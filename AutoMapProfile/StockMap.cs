using AutoMapper;
using stockserver.Models.View;
using StockServer.Models.DataBase;

namespace stockserver.AutoMapProfile
{
    public class StockMap : Profile
    {
        public StockMap()
        {
            CreateMap<Stock, StockCalculateModel>()
                .ForMember(sm => sm.percent, ie => ie.Ignore())
                .ForMember(sm => sm.sum_percent_n, ie => ie.Ignore());
        }
    }
}