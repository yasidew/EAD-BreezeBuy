/*
 * VendorController.cs
 * Author: [Dayananda I.H.M.B.L. | IT21307058]
 * The VendorController manages vendor-related operations, 
   including creating, updating, retrieving, and deleting 
   vendor profiles, as well as handling customer feedback. 
   It also provides functionality to fetch vendor details, 
   sort vendors by ratings, and allow customers to edit and 
   view their feedback.
 
 */


using BreezeBuy.Models;
using BreezeBuy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BreezeBuy.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class VendorController : ControllerBase
	{
		private readonly VendorService _vendorService;

		public VendorController(VendorService vendorService)
		{
			_vendorService = vendorService;
		}

		// Get all vendors: Retrieves a list of all vendorss: GET api/vendor
		[HttpGet]
		public async Task<ActionResult<List<Vendor>>> Get()
		{
			var vendors = await _vendorService.GetAsync();
			return Ok(vendors);
		}

		// Retrieves vendor details using the userId: GET api/vendor/{id}
		[HttpGet("{userId:length(24)}", Name = "GetVendor")]
		public async Task<ActionResult<Vendor>> GetById(string userId)
		{
			var vendor = await _vendorService.GetByUserIdAsync(userId);
			if (vendor == null)
			{
				return NotFound();
			}
			return Ok(vendor);
		}

		// Retrieves vendor details using the vendor's unique id: GET api/vendor/{id}
		[HttpGet("v1/{id:length(24)}", Name = "GetVendorDetail")]
		public async Task<ActionResult<Vendor>> GetOneById(string id)
		{
			var vendor = await _vendorService.GetByIdAsync(id);
			if (vendor == null)
			{
				return NotFound();
			}
			return Ok(vendor);
		}

		// Create new vendor: POST api/vendor
		[HttpPost]
		public async Task<ActionResult<Vendor>> Create([FromBody] Vendor newVendor)
		{
			await _vendorService.CreateAsync(newVendor);
			return CreatedAtRoute("GetVendorDetail", new { id = newVendor.Id.ToString() }, newVendor);
		}

		// Updates an existing vendor's details: PUT api/vendor/{id}
		[HttpPut("{id:length(24)}")]
		public async Task<ActionResult> Update(string id, Vendor updatedVendor)
		{
			var vendor = await _vendorService.GetByIdAsync(id);
			if (vendor == null)
			{
				return NotFound();
			}
			await _vendorService.UpdateAsync(id, updatedVendor);
			return Ok(new { message = "Vendor updated successfully" });
		}

		// Deletes a vendor based on the vendor id: DELETE api/vendor/{id}
		[HttpDelete("{id:length(24)}")]
		public async Task<ActionResult> Delete(string id)
		{
			var vendor = await _vendorService.GetByIdAsync(id);
			if (vendor == null)
			{
				return NotFound();
			}
			await _vendorService.RemoveAsync(id);
			return Ok(new { message = "Vendor deleted successfully" });
		}

		//Adds feedback from a customer to a vendor: POST api/vendor/{vendorId}/feedback
		[HttpPost("{vendorId}/feedback")]
		public async Task<IActionResult> AddFeedback(string vendorId, [FromBody] Comment feedback)
		{
			var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(customerId))
			{
				return Unauthorized();
			}

			feedback.CustomerId = customerId;
			feedback.IsCommentEditable = true;

			await _vendorService.AddFeedbackAsync(vendorId, feedback);
			return Ok(new { message = "Feedback added successfully" });
		}

		// Edits an existing customer feedback comment: PUT api/vendor/{vendorId}/feedback/{commentId}
		[HttpPut("{vendorId}/feedback/{commentId}")]
		public async Task<IActionResult> EditComment(string vendorId, string commentId, [FromBody] string updatedComment)
		{
			var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(customerId))
			{
				return Unauthorized();
			}

			await _vendorService.EditCommentAsync(vendorId, commentId, updatedComment);
			return Ok(new { message = "Comment updated successfully" });
		}


		// Retrieves a vendor and their associated customer feedbacks: GET api/vendor/{vendorId}
		[HttpGet("{vendorId}")]
		public async Task<ActionResult<Vendor>> GetVendor(string vendorId)
		{
			var vendor = await _vendorService.GetVendorWithFeedbackAsync(vendorId);
			if (vendor == null)
			{
				return NotFound();
			}
			return Ok(vendor);
		}

		//Retrieves all feedbacks given by the logged-in customer
		[HttpGet("customer/feedbacks")]
		[Authorize]
		public async Task<IActionResult> GetCustomerFeedbacks()
		{
			var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(customerId))
			{
				return Unauthorized();
			}

			var feedbacks = await _vendorService.GetCustomerFeedbacksAsync(customerId);

			if (feedbacks == null || !feedbacks.Any())
			{
				return NotFound(new { message = "No feedbacks found for this customer" });
			}

			return Ok(feedbacks);
		}

		// Retrieves a specific comment based on vendorId and commentId: GET api/vendor/{vendorId}/feedback/{commentId}
		[HttpGet("{vendorId}/feedback/{commentId}")]
		public async Task<IActionResult> GetFeedback(string vendorId, string commentId)
		{
			var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(customerId))
			{
				return Unauthorized();
			}

			var comment = await _vendorService.GetFeedbackAsync(vendorId, commentId);

			if (comment == null)
			{
				return NotFound(new { message = "Comment not found" });
			}

			return Ok(comment);
		}

		// Retrieves vendors sorted by their average rating
		[HttpGet("sorted-vendors")]
		public async Task<IActionResult> GetSortedVendors()
		{
			var sortedVendors = await _vendorService.GetVendorsSortedByRatingAsync();
			return Ok(sortedVendors);
		}
	}
}
