using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CollectionsApp.Models;
using Microsoft.AspNetCore.Identity;
using CollectionsApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Markdig;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;

namespace CollectionsApp.Controllers
{
    [Authorize(Roles = "active user")]
    public class CollectionsController : Controller
    {
        private const string _fileDirectoryId = "1GEKT4vXhFam7O8O5c27ifoz0XG2F1r2M";
        private const string _clientEmail = "valerya@collectionsapp-366222.iam.gserviceaccount.com";
        private const string _pathToServiceAccountCredentials = "credentials.json";

        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationContext _db;
        private readonly IWebHostEnvironment _appEnvironment;

        public CollectionsController(UserManager<IdentityUser> userManager,
            ApplicationContext db, IWebHostEnvironment appEnvironment)
        {
            _userManager = userManager;
            _db = db;
            _appEnvironment = appEnvironment;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult CollectionsList(string collectionsOwnerId)
        {
            ViewBag.collectionsOwnerId = collectionsOwnerId;
            return View();
        }

        [HttpGet]
        public IActionResult CreateCollection(string collectionsOwnerId)
        {
            ViewBag.collectionFields = null;
            ViewBag.collectionsOwnerId = collectionsOwnerId;
            return View(new CreateCollectionViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateCollection(string collectionsOwnerId, 
            CreateCollectionViewModel model, List<CollectionField> collectionFields, IFormFile collectionImage)
        {
            if (ModelState.IsValid & IsCollectionTitleUnique(collectionsOwnerId, model.Title) &
                    !ContainsCollectionFieldTitlesDuplicates(collectionFields) )
            {
                int addedCollectionId = await AddCollectionToDb(collectionsOwnerId, model, collectionFields);
                if(collectionImage != null)
                {
                    string uploadedImageId = await UploadImageToGoogleDrive(collectionImage, addedCollectionId);
                    UpdateCollectionImageIdInDb(addedCollectionId, uploadedImageId);
                }
                return RedirectToAction("CollectionsList", new { collectionsOwnerId = collectionsOwnerId });
            }
            ViewBag.collectionFields = collectionFields;
            ViewBag.collectionsOwnerId = collectionsOwnerId;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCollection(int collectionId)
        {
            Collection collectionToEdit = await _db.Collections.FindAsync(collectionId);
            var model = new EditCollectionViewModel(collectionToEdit);
            ViewBag.collection = collectionToEdit;
            return View(model);
        }

        [HttpPost]
        public IActionResult EditCollection(int collectionId, EditCollectionViewModel model)
        {
            Collection collectionToEdit = _db.Collections.Find(collectionId);
            if (ModelState.IsValid & IsEditedCollectionTitleUnique(collectionToEdit.UserId, collectionToEdit.Title, model.Title))
            {
                UpdateCollectionInDb(collectionToEdit, model);
                return RedirectToAction("CollectionsList", new { collectionsOwnerId = collectionToEdit.UserId });
            }
            ViewBag.collection = collectionToEdit;
            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteCollection(int collectionId)
        {
            Collection collectionToDelete = _db.Collections.Find(collectionId);
            if (collectionToDelete != null)
            {
                _db.Collections.Remove(collectionToDelete);
                _db.SaveChanges();
            }
            return RedirectToAction("CollectionsList", new { collectionsOwnerId = collectionToDelete.UserId});
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult GetCollectionsList(string userId)
        {
            var userCollections = _db.Collections.Where(c => c.UserId.Equals(userId)).ToList();
            foreach(var collection in userCollections)
            {
                string htmlDescription = Markdown.ToHtml(collection.Description);
                collection.Description = htmlDescription;
            }
            return Json(userCollections);
        }


        private bool IsCollectionTitleUnique(string userId, string collectionTitle)
        {
            var userCollections = _db.Collections.Where(c => c.UserId.Equals(userId)).ToList();
            foreach(var collection in userCollections)
            {
                if (collection.Title.Equals(collectionTitle))
                {
                    ModelState.AddModelError(string.Empty, "Collection title \"" + collectionTitle + "\" is already taken.");
                    return false;
                }
            }
            return true;
        }

        
        private bool IsEditedCollectionTitleUnique(string userId, string previousCollectionTitle, 
            string newCollectionTitle)
        {
            if (previousCollectionTitle.Equals(newCollectionTitle))
            {
                return true;
            } else
            {
                return IsCollectionTitleUnique(userId, newCollectionTitle);
            }
        }

        private bool ContainsCollectionFieldTitlesDuplicates(List<CollectionField> collectionFields)
        {
            var fieldTitlesList = new List<string>(
                            new string[] { "Title", "Tags" }
                        );
            foreach (var collectionField in collectionFields)
            {
                fieldTitlesList.Add(collectionField.Title);
            }
            fieldTitlesList.RemoveAll(s => s == null);
            bool containsDuplicates = fieldTitlesList.Distinct().Count() != fieldTitlesList.Count;
            if (containsDuplicates)
            {
                ModelState.AddModelError(string.Empty, "Collection field titles can't contain duplicates.");
            }
            return containsDuplicates;
        }

        private async Task<int> AddCollectionToDb(string collectionsOwnerId,
            CreateCollectionViewModel model, List<CollectionField> collectionFields)
        {
            var collectionToAdd = new Collection(collectionsOwnerId, model, collectionFields);
            await _db.Collections.AddAsync(collectionToAdd);
            _db.SaveChanges();
            return collectionToAdd.Id;
        }

        private void UpdateCollectionInDb(Collection collectionToEdit, EditCollectionViewModel model)
        {
            EditCollectionFieldValues(collectionToEdit, model);
            _db.Collections.Update(collectionToEdit);
            _db.SaveChanges();
        }

        
        private void EditCollectionFieldValues(Collection collectionToEdit, EditCollectionViewModel model)
        {
            collectionToEdit.Title = model.Title;
            collectionToEdit.Theme = model.Theme;
            collectionToEdit.Description = model.Description;
        }


        private async Task<string> UploadImageToWebRoot(IFormFile imageToUpload)
        {
            string path = null;
            if (imageToUpload != null)
            {
                path = _appEnvironment.WebRootPath + "/images/" + imageToUpload.FileName;
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await imageToUpload.CopyToAsync(fileStream);
                }
            }
            return path;
        }

        private async Task<string> UploadImageToGoogleDrive(IFormFile imageToUpload, int collectionId)
        {
            string imageId = null;
            if(imageToUpload != null)
            {
                string pathToImage = await UploadImageToWebRoot(imageToUpload);

                var credential = GoogleCredential.FromFile(_pathToServiceAccountCredentials).
                        CreateScoped(DriveService.ScopeConstants.Drive);

                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential
                });

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = collectionId + imageToUpload.FileName,
                    Parents = new List<string>() { _fileDirectoryId }
                };

                // Create a new file on Google Drive
                await using (var fsSource = new FileStream(pathToImage, FileMode.Open, FileAccess.Read))
                {
                    // Create a new file, with metadata and stream.
                    var request = service.Files.Create(fileMetadata, fsSource, imageToUpload.ContentType);
                    request.Fields = "*";
                    var results = await request.UploadAsync(CancellationToken.None);

                    // the file id of the new file we created
                    imageId = request.ResponseBody?.Id;
                }

                System.IO.File.Delete(pathToImage);
            }
            return imageId;
        }

        private void UpdateCollectionImageIdInDb(int collectionId, string uploadedImageId)
        {
            var collection = _db.Collections.Find(collectionId);
            collection.ImageUrl = uploadedImageId;
            _db.Collections.Update(collection);
            _db.SaveChanges();
        }
    }
}
