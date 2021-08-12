using System;
using Application.DTOs.Account.Route;
using Domain.Models.Version2;

namespace Application.DTOs.Certificate
{
    public class CertificateDTO
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string Title { get; set; }
        public int LifeTime { get; set; }
        public string Template { get; set;}
        public virtual RouteDTO Route { get; set; }
    }
}