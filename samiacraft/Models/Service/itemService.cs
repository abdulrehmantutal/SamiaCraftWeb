using samiacraft.Models.BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace samiacraft.Models.Service
{
    public class itemService : baseService
    {
        itemBLL _service;
        //blogBLL _blogBLL;
        public itemService()
        {
            _service = new itemBLL();
            //_blogBLL = new blogBLL();
        }

        public List<itemBLL> GetAll(int LocationID)
        {
            try
            {
                return _service.GetAll(LocationID);
            }
            catch (Exception ex)
            {
                return new List<itemBLL>();
            }
        }
        //public List<blogBLL> GetAllBlog(int LocationID)
        //{
        //    try
        //    {
        //        return _blogBLL.GetAll(LocationID);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<blogBLL>();
        //    }
        //}
        public itemBLL GetSelecteditems(int ID, int LocationID)
        {
            try
            {
                return _service.GetSelecteditems(ID, LocationID);
            }
            catch (Exception ex)
            {
                return new itemBLL();
            }
        }
        public List<itemBLL> GetAllFeatured()
        {
            try
            {
                return _service.GetAllFeatured();
            }
            catch (Exception ex)
            {
                return new List<itemBLL>();
            }
        }

        public List<itemBLL> GetAllPopular()
        {
            try
            {
                return _service.GetAllPopular();
            }
            catch (Exception ex)
            {
                return new List<itemBLL>();
            }
        }

        public List<itemBLL> GetAllValentineDay()
        {
            try
            {
                return _service.GetAllValentineDay();
            }
            catch (Exception ex)
            {
                return new List<itemBLL>();
            }
        }

    }
}