using samiacraft.Models.BLL;

namespace samiacraft.Models.Service
{
    public class blogfilterService
    {
        blogfilterBLL _service;
        public blogfilterService()
        {
            _service = new blogfilterBLL();
        }
        public List<blogfilterBLL> GetAll(blogfilterBLL filter)
        {
            try
            {
                return _service.GetAll(filter);
            }
            catch (Exception ex)
            {
                return new List<blogfilterBLL>();
            }
        }
    }
}
