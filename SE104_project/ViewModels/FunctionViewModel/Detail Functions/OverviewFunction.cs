﻿using SE104_OnlineShopManagement.Models.ModelEntity;
using SE104_OnlineShopManagement.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace SE104_OnlineShopManagement.ViewModels.FunctionViewModel.Detail_Functions
{
    public class OverviewFunction : BaseFunction
    {
        public OverviewFunction(AppSession session, MongoConnect connect) : base(session, connect)
        {
        }
    }
}