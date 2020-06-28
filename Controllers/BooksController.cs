using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// book controller to interact with books table[
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository, ILoggerService logger, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// get all books
        /// </summary>
        /// <returns>list of books</returns>
        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            try
            {
                _logger.LogInfo("Attempted Get All Books");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo("Successfully got all books");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError("Controller: " + GetControllerActionNames() + " Error: " + ex.ToString());
            }
        }

        /// <summary>
        /// get a book by id
        /// </summary>
        /// <returns>book</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            try
            {
                _logger.LogInfo("Attempted Get Book: " + id);
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogWarn("Could not find book: " + id);
                    return NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                _logger.LogInfo("Successfully got book " + id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError("Controller: " + GetControllerActionNames() + " Error: " + ex.ToString());
            }
        }

        /// <summary>
        /// create a book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            try
            {
                _logger.LogInfo("Attempted to submit book ");

                if (bookDTO == null)
                {
                    _logger.LogWarn("book Create is NULL!");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("book Create model state is incomplete!");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);

                var isSuccess = await _bookRepository.Create(book);

                if (!isSuccess)
                {
                    _logger.LogWarn("book Create failed!");
                    return InternalError("book Create failed!");
                }

                _logger.LogInfo("Successfully created book " + bookDTO.Title);
                return Created("Create", new { book });
            }
            catch (Exception ex)
            {
                return InternalError("Controller: " + GetControllerActionNames() + " Error: " + ex.ToString());
            }
        }

        /// <summary>
        /// update a book
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            try
            {
                _logger.LogInfo("Attempted to update Book with ID: " + id);

                var bookcheck = await _bookRepository.isExist(id);

                if (id < 1 || id != bookDTO.ID || bookDTO == null || !bookcheck)
                {
                    _logger.LogWarn("Book update is NULL or ID is invalid!");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Book Update model state is incomplete!");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);

                var isSuccess = await _bookRepository.Update(book);

                if (!isSuccess)
                {
                    _logger.LogWarn("Book Update failed!");
                    return InternalError("Book Update failed!");
                }

                _logger.LogInfo("Successfully updated book " + bookDTO.Title);
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError("Controller: " + GetControllerActionNames() + " Error: " + ex.ToString());
            }
        }

        /// <summary>
        /// delete a book
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInfo("Attempted to delete Book with ID: " + id);

                if (id < 1)
                {
                    _logger.LogWarn("Author delete is NULL or ID is invalid!");
                    return BadRequest();
                }

                var book = await _bookRepository.FindById(id);

                if (book == null)
                    return NotFound();

                var isSuccess = await _bookRepository.Delete(book);

                if (!isSuccess)
                {
                    _logger.LogWarn("Author delete failed!");
                    return InternalError("Author delete failed!");
                }

                _logger.LogInfo("Successfully deleted book " + book.Title + " ID: " + id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError("Controller: " + GetControllerActionNames() + " Error: " + ex.ToString());
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return (controller + " - " + action);
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, message);
        }
    }
}
