using Dmc.Cms.Model;
using Entangled.Life.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Entangled.Life.Web.Mappers
{
    public static class ImageMapper
    {
        public static void TransferToViewModel(Image source, ImageViewModel destination)
        {
            destination.AltText = source.AltText;
            destination.Description = source.Caption;
            destination.LargeImage = source.LargeImage;
            destination.Name = source.Name;
            destination.SmallImage = source.SmallImage;
            destination.Id = source.Id;
        }
    }
}