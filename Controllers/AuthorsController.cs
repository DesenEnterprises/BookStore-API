using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// endpoint for authors
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public AuthorsController(IAuthorRepository authorRepository, ILoggerService logger, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// get all authors
        /// </summary>
        /// <returns>List of authors</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Attempted Get All Authors");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.LogInfo("Successfully got all authors");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError("Controller: " + GetControllerActionNames() + " Error: " + ex.ToString());
            }            
        }

        /// <summary>
        /// get an author by id
        /// </summary>
        /// <returns>List of authors</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo("Attempted Get Author " + id);
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn("Could not find author: " + id);
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.LogInfo("Successfully got author " + id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError("Controller: " + GetControllerActionNames() + " Error: " + ex.ToString());
            }
        }

        /// <summary>
        /// create an author
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo("Attempted to submit Author ");

                if (authorDTO == null)
                {
                    _logger.LogWarn("Author Create is NULL!");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Author Create model state is incomplete!");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);

                var isSuccess = await _authorRepository.Create(author);

                if(!isSuccess)
                {
                    _logger.LogWarn("Author Create failed!");
                    return InternalError("Author Create failed!");
                }

                _logger.LogInfo("Successfully created author " + authorDTO.Firstname + " " + authorDTO.Lastname);
                return Created("Create", new { author });
            }
            catch (Exception ex)
            {
                return InternalError("Controller: " + GetControllerActionNames() + " Error: " + ex.ToString());
            }
        }

        /// <summary>
        /// update an author
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                _logger.LogInfo("Attempted to update Author with ID: " + id);

                var authorcheck = await _authorRepository.isExist(id);

                if (id < 1 || id != authorDTO.ID || authorDTO == null || !authorcheck)
                {
                    _logger.LogWarn("Author update is NULL or ID is invalid!");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Author Update model state is incomplete!");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDTO);

                var isSuccess = await _authorRepository.Update(author);

                if (!isSuccess)
                {
                    _logger.LogWarn("Author Update failed!");
                    return InternalError("Author Update failed!");
                }

                _logger.LogInfo("Successfully updated author " + authorDTO.Firstname + " " + authorDTO.Lastname);
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError("Controller: " + GetControllerActionNames() + " Error: " + ex.ToString());
            }
        }

        /// <summary>
        /// delete an author
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInfo("Attempted to delete Author with ID: " + id);

                if (id < 1 )
                {
                    _logger.LogWarn("Author delete is NULL or ID is invalid!");
                    return BadRequest();
                }

                var author = await _authorRepository.FindById(id);

                if (author == null)
                    return NotFound();

                var isSuccess = await _authorRepository.Delete(author);

                if (!isSuccess)
                {
                    _logger.LogWarn("Author delete failed!");
                    return InternalError("Author delete failed!");
                }

                _logger.LogInfo("Successfully deleted author " + author.Firstname + " " + author.Lastname + "ID: " + id);
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
