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

		// Get all vendors: GET api/vendor
		[HttpGet]
		public async Task<ActionResult<List<Vendor>>> Get()
		{
			var vendors = await _vendorService.GetAsync();
			return Ok(vendors);
		}

		// Get vendor by id: GET api/vendor/{id}
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

		// Get vendor by id: GET api/vendor/{id}
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
		public async Task<ActionResult<Vendor>> Create(Vendor newVendor)
		{
			await _vendorService.CreateAsync(newVendor);
			return CreatedAtRoute("GetVendor", new { id = newVendor.Id.ToString() }, newVendor);
		}

		// Update vendor: PUT api/vendor/{id}
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

		// Delete vendor: DELETE api/vendor/{id}
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

		// POST api/vendor/{vendorId}/feedback
		[HttpPost("{vendorId}/feedback")]
		public async Task<IActionResult> AddFeedback(string vendorId, [FromBody] Comment feedback)
		{
			// Get customer ID from logged-in user claims
			var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(customerId))
			{
				return Unauthorized();
			}

			// Set the feedback's customer ID and initial editable status
			feedback.CustomerId = customerId;
			feedback.IsCommentEditable = true; // Comment is editable when first added

			await _vendorService.AddFeedbackAsync(vendorId, feedback);
			return Ok(new { message = "Feedback added successfully" });
		}

		// PUT api/vendor/{vendorId}/feedback/{commentId}
		[HttpPut("{vendorId}/feedback/{commentId}")]
		public async Task<IActionResult> EditComment(string vendorId, string commentId, [FromBody] string updatedComment)
		{
			// Ensure the user editing is the owner of the comment
			var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(customerId))
			{
				return Unauthorized();
			}

			await _vendorService.EditCommentAsync(vendorId, commentId, updatedComment);
			return Ok(new { message = "Comment updated successfully" });
		}


		// GET api/vendor/{vendorId}
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

		[HttpGet("customer/feedbacks")]
		[Authorize]
		public async Task<IActionResult> GetCustomerFeedbacks()
		{
			// Get customer ID from logged-in user claims
			var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(customerId))
			{
				return Unauthorized();
			}

			// Fetch customer feedbacks
			var feedbacks = await _vendorService.GetCustomerFeedbacksAsync(customerId);

			if (feedbacks == null || !feedbacks.Any())
			{
				return NotFound(new { message = "No feedbacks found for this customer" });
			}

			return Ok(feedbacks);
		}

		// GET api/vendor/{vendorId}/feedback/{commentId}
		[HttpGet("{vendorId}/feedback/{commentId}")]
		public async Task<IActionResult> GetFeedback(string vendorId, string commentId)
		{
			// Ensure the user is authenticated
			var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(customerId))
			{
				return Unauthorized();
			}

			// Fetch the feedback using the service
			var comment = await _vendorService.GetFeedbackAsync(vendorId, commentId);

			if (comment == null)
			{
				return NotFound(new { message = "Comment not found" });
			}

			// Return the feedback as a response
			return Ok(comment);
		}
	}
}
