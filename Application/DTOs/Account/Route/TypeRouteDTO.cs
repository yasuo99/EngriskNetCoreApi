using System.Collections.Generic;

namespace Application.DTOs.Account.Route
{
    public class TypeRouteDTO
    {
        public TypeRouteDTO()
        {
            Engrisk = new List<RouteLearnDTO>();
            Private = new List<RouteLearnDTO>();
        }
        public List<RouteLearnDTO> Engrisk { get; set; }
        public List<RouteLearnDTO> Private { get; set; }
        public RouteLearnDTO LastRoute { get; set; }
    }
}