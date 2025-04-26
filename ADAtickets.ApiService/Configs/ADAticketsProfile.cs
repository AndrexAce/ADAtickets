using ADAtickets.ApiService.Dtos;
using ADAtickets.ApiService.Models;
using AutoMapper;

namespace ADAtickets.ApiService.Configs
{
    /// <summary>
    /// Defines the AutoMapper profile to map the entities to DTOs and vice versa.
    /// </summary>
    class ADAticketsProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ADAticketsProfile"/> class.
        /// </summary>
        public ADAticketsProfile()
        {
            CreateMap<Edit, EditDto>();
            CreateMap<EditDto, Edit>();
        }
    }
}
