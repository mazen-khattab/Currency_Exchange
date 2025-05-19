using AutoMapper;
using CurrencyExchange_Practice.Application.Services;
using CurrencyExchange_Practice.Core.DTOs.NewFolder;
using CurrencyExchange_Practice.Core.Entities;
using CurrencyExchange_Practice.Core.Interfaces;
using CurrencyExchange_Practice.Core.OtherObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CurrencyExchange_Practice.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        readonly IMapper _mapper;
        readonly ICountryService _countryService;

        public CountryController(ICountryService countryService, IMapper mapper)
        {
            _countryService = countryService;
            _mapper = mapper;
        }

        [HttpGet]
        [ResponseCache(Duration = 30)]
        [Route("GetAll")]
        [Authorize(Roles = $"{StaticUserRoles.USER}")]
        public async Task<ActionResult<Country>> Getall() => Ok(await _countryService.GetAllAsync(tracked: false));

        [HttpGet("GetCountriesByCode")]
        public async Task<ActionResult<IEnumerable<Country>>> GetAllCountriesByCode(string code) => Ok(await _countryService.GetAllCountriesByCode(code));

        [HttpGet("GetAllCountriesPaginated")]
        public async Task<ActionResult<IEnumerable<Country>>> GetAllCountriesPaginated([FromQuery] int pageNumber, int pageSize)
            => Ok(await _countryService.GetAllCountriesPaginated(pageNumber, pageSize));

        [HttpGet("id")]
        [Authorize(Roles = StaticUserRoles.USER)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Country>> Get([FromQuery] int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var country = await _countryService.GetByIdAsync(id);

            return country != null ? Ok(country) : NotFound();
        }


        [HttpPost("create")]
        [Authorize(Roles = $"{StaticUserRoles.ADMIN},{StaticUserRoles.OWNER}")]
        public async Task<ActionResult> Add([FromBody] CountryDTO countryDTO)
        {
            var country = _mapper.Map<Country>(countryDTO);
            await _countryService.AddAsync(country);
            return Ok(country);
        }


        [HttpPut]
        [Route("update")]
        [Authorize(Roles = $"{StaticUserRoles.ADMIN},{StaticUserRoles.OWNER}")]
        public async Task<ActionResult> Update([FromBody] CountryDTO newCountry, int id)
        {
            var oldCountry = await _countryService.GetByIdAsync(id);

            if (!(oldCountry is { }))
            {
                return NotFound();
            }

            _mapper.Map(newCountry, oldCountry);

            await _countryService.UpdateAsync(oldCountry);

            return Ok(newCountry);
        }


        [HttpDelete]
        [Route("delete")]
        [Authorize(Roles = $"{StaticUserRoles.ADMIN},{StaticUserRoles.OWNER}")]
        public async Task<ActionResult> Delete(int id)
        {
            var country = await _countryService.GetByIdAsync(id);

            if (!(country is { }))
            {
                return NotFound();
            }

            await _countryService.DeleteAsync(country);

            return Ok();
        }
    }
}
