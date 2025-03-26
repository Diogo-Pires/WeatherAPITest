using Application.DTOs;

namespace Application.Interfaces;

public interface IOpenWeatherMapService
{
    Task<Root?> GetWeatherContentAsync();
}