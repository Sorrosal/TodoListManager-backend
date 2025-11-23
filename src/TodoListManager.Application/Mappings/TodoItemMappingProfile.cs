// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using AutoMapper;
using TodoListManager.Application.DTOs;
using TodoListManager.Domain.Entities;
using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping between Domain entities and DTOs.
/// </summary>
public sealed class TodoItemMappingProfile : Profile
{
    public TodoItemMappingProfile()
    {
        CreateMap<TodoItem, TodoItemDto>()
            .ForMember(dest => dest.TotalProgress, opt => opt.MapFrom(src => src.GetTotalProgress()))
            .ForMember(dest => dest.LastProgressionDate, opt => opt.MapFrom(src => src.GetLastProgressionDate()))
            .ForMember(dest => dest.Progressions, opt => opt.MapFrom(src => src.Progressions));

        CreateMap<Progression, ProgressionDto>();
    }
}
