//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SklRcmApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class upload
    {
        public int up_id { get; set; }
        public string up_filename { get; set; }
        public string up_user { get; set; }
        public System.DateTime up_date { get; set; }
        public string up_path { get; set; }
        public Nullable<int> up_apply { get; set; }
        public string up_message { get; set; }
        public long up_size { get; set; }
    }
}