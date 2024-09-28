using BreezeBuy.Dto;
using BreezeBuy.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BreezeBuy.Services
{
	public class VendorService
	{
		private readonly IMongoCollection<Vendor> _vendorCollection;

		public VendorService(IOptions<MongoDbSettings> mongoDbSettings)
		{
			var settings = mongoDbSettings.Value;
			var client = new MongoClient(settings.ConnectionString);
			var database = client.GetDatabase(settings.DatabaseName);
			_vendorCollection = database.GetCollection<Vendor>(settings.VendorCollectionName);
		}

		// Get all vendors
		public async Task<List<Vendor>> GetAsync() =>
			await _vendorCollection.Find(vendor => true).ToListAsync();

		// Get vendor by id
		public async Task<Vendor> GetByUserIdAsync(string userId) =>
			await _vendorCollection.Find(vendor => vendor.UserId == userId).FirstOrDefaultAsync();

		public async Task<Vendor> GetByIdAsync(string id) =>
			await _vendorCollection.Find(vendor => vendor.Id == id).FirstOrDefaultAsync();

		// Create a new vendor
		public async Task CreateAsync(Vendor newVendor) =>
			await _vendorCollection.InsertOneAsync(newVendor);

		// Update vendor
		public async Task UpdateAsync(string id, Vendor updatedVendor)
		{
			updatedVendor.Id = id; // Ensure the Id is set correctly
			await _vendorCollection.ReplaceOneAsync(vendor => vendor.Id == id, updatedVendor);
		}

		// Delete vendor
		public async Task RemoveAsync(string id) =>
			await _vendorCollection.DeleteOneAsync(vendor => vendor.Id == id);

		// Add ranking and comment to a vendor
		public async Task AddFeedbackAsync(string vendorId, Comment feedback)
		{
			var vendor = await _vendorCollection.Find(v => v.Id == vendorId).FirstOrDefaultAsync();
			if (vendor != null)
			{
				vendor.Comments.Add(feedback);
				vendor.AverageRating = vendor.Comments.Average(f => f.Rank);
				await _vendorCollection.ReplaceOneAsync(v => v.Id == vendorId, vendor);
			}
		}


		// Update comment only (rank remains the same)
		public async Task EditCommentAsync(string vendorId, string commentId, string updatedComment)
		{
			var vendor = await _vendorCollection.Find(v => v.Id == vendorId).FirstOrDefaultAsync();
			var comment = vendor?.Comments.FirstOrDefault(c => c.Id == commentId);

			if (comment != null && comment.IsCommentEditable)
			{
				comment.CommentText = updatedComment;
				await _vendorCollection.ReplaceOneAsync(v => v.Id == vendorId, vendor);
			}
		}

		// Helper method to calculate average rating
		private double CalculateAverageRating(List<Comment> comments)
		{
			if (comments == null || comments.Count == 0) return 0;
			return comments.Average(c => c.Rank);
		}

		// Get vendor details including feedback
		public async Task<Vendor> GetVendorWithFeedbackAsync(string vendorId)
		{
			return await _vendorCollection.Find(v => v.Id == vendorId).FirstOrDefaultAsync();
		}

		public async Task<List<CustomerFeedbackDto>> GetCustomerFeedbacksAsync(string customerId)
		{
			var filter = Builders<Vendor>.Filter.ElemMatch(v => v.Comments, c => c.CustomerId == customerId);
			var vendorComments = await _vendorCollection.Find(filter).ToListAsync();

			var customerFeedbacks = vendorComments
				.SelectMany(vendor => vendor.Comments
					.Where(comment => comment.CustomerId == customerId)
					.Select(comment => new CustomerFeedbackDto
					{
						VendorId = vendor.Id,
						VendorName = vendor.Name,
						VendorProduct = vendor.Product,
						CommentId = comment.Id,
						CommentText = comment.CommentText,
						Rank = comment.Rank,
						IsCommentEditable = comment.IsCommentEditable
					}))
				.ToList();

			return customerFeedbacks;
		}

		public async Task<Comment> GetFeedbackAsync(string vendorId, string commentId)
		{
			var vendor = await _vendorCollection.Find(v => v.Id == vendorId).FirstOrDefaultAsync();
			if (vendor != null)
			{
				// Find the comment with the specified commentId
				var comment = vendor.Comments.FirstOrDefault(c => c.Id == commentId);
				return comment;
			}
			return null; // Return null if vendor or comment doesn't exist
		}


	}

}
