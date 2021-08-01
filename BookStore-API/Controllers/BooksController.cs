using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{

    /// <summary>
    /// Interact with book table
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository,
                                ILoggerService loggerService,
                                IMapper mapper)

        {
            _bookRepository = bookRepository;
            _loggerService = loggerService;
            _mapper = mapper;
        }



        /// <summary>
        /// Get all books
        /// </summary>
        /// <returns>Return a list of books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location} : Attempted to retrive all books.");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _loggerService.LogInfo($"{location} : successfully returns all book.");
                return Ok(response);

            }
            catch (Exception e)
            {

                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        /// <summary>
        /// Get's a book with specific id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns a book</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location} \t: Attempted call for id: {id}");
                var book = await _bookRepository.FindByID(id);
                var response = _mapper.Map<BookDTO>(book);
                if (book == null)
                {
                    _loggerService.LogError($"{location} : Failed to retrive record with id: {id}");
                    return NotFound();
                }
                _loggerService.LogInfo($"{location} : successfully got record with id: {id}");
                return Ok(response);

            }
            catch (Exception e)
            {

                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        /// <summary>
        /// Create a neww book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location} : Create a new book attempted.");
                if (bookDTO == null)
                {
                    _loggerService.LogWarn($"{location} : empty book creation attempted");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _loggerService.LogWarn($"{location}: Invalid request submitted.");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);

                if (!isSuccess)
                {
                    return InternalError($"{location} : Creation failed.");
                }


                _loggerService.LogInfo($"{location} : Creation Successfull.");
                _loggerService.LogInfo($"{location} : {book}");
                return Created("Create", new { book });
            }
            catch (Exception e)
            {

                return InternalError($"{location}: {e.Message} - {e.InnerException}");
            }
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location} : Update attempted with book id : {id}");
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                {
                    _loggerService.LogInfo($"{location} : Update attempted with bad data.");
                    return BadRequest();
                }

                var isExists = await _bookRepository.isExists(id);
                if (!isExists)
                {
                    _loggerService.LogError($"{location} Book record not found with id: {id}");
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    _loggerService.LogError($"{location} : Invalid book information for update.");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Update(book);
                if (!isSuccess)
                {
                    return InternalError($"{location} : Update failed.");
                }

                _loggerService.LogInfo($"{location} : Book record with id : {id} is updated.");
                return NoContent();
            }
            catch (Exception e)
            {

                return InternalError($"{location} : {e.Message} - {e.InnerException}");
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _loggerService.LogInfo($"{location} : Delete attempted on record with id: {id}");
                if (id < 1)
                {
                    _loggerService.LogError($"{location} : Delete failed with bad data - id: {id}");
                    return BadRequest();
                }

                var isExists = await _bookRepository.isExists(id);
                if (!isExists)
                {
                    _loggerService.LogError($"{location} : Delete failed, No record found with id:{id}");
                    return NotFound();
                }

                var book = await _bookRepository.FindByID(id);
                if (book == null)
                {
                    _loggerService.LogError($"{location} : Delete failed, No record found with id: {id}");
                    return NotFound();
                }
                var isSuccess = await _bookRepository.Delete(book);
                if (!isSuccess)
                {
                    return InternalError($"{location } : Delete failed with book id: {id}");
                }

                _loggerService.LogInfo($"{location} : Delete successful, book record with id: {id}");
                return NoContent();

            }
            catch (Exception e)
            {

                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";
        }

        private ObjectResult InternalError(string message)
        {
            _loggerService.LogError(message);
            return StatusCode(500, "Some thing went wrong, Please contact with administrator.");
        }
    }
}
