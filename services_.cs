//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace lab
{
    using System;
    using System.Collections.Generic;
    
    public partial class services_
    {
        public int Код { get; set; }
        public Nullable<int> user_id { get; set; }
        public Nullable<int> services { get; set; }
    
        public virtual users_ users_ { get; set; }
        public virtual Услуги_ Услуги_ { get; set; }
    }
}
