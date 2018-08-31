using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace digiCamControl.LightBox.Core.Clasess
{
    public class ValueItemCollection
    {
        public delegate void ValueChangedEventHandler(object sender, ValueItem item);
        public event ValueChangedEventHandler ValueChangedEvent;



        public List<ValueItem> Items { get; set; }

        public ValueItemCollection()
        {
            Items = new List<ValueItem>();
        }

        public object this[string index]
        {
            get { return Get(index)?.Value; }
            set
            {
                var item = Get(index);
                if (item == null)
                {
                    item = new ValueItem() {Name = index};
                    Items.Add(item);
                }
                if (item.Value != TransformToString(value))
                {
                    item.Value = TransformToString(value);
                }
                OnValueChangedEvent(item);
            }
        }

        public ValueItem Get(string name)
        {
            foreach (var item in Items)
            {
                if (item.Name == name)
                    return item;
            }
            return null;
        }

        public int GetInt(string name)
        {
            var val = Get(name);
            if (val == null)
                return 0;
            return Convert.ToInt32(val.Value, CultureInfo.InvariantCulture);
        }

        public double GetDouble(string name)
        {
            var val = Get(name);
            if (val == null)
                return 0;
            return Convert.ToDouble(val.Value, CultureInfo.InvariantCulture);
        }

        public bool GetBool(string name)
        {
            var val = Get(name);
            if (val == null)
                return false;
            return val.Value=="True";
        }

        public string TransformToString(object o)
        {
            if (o is int)
                return ((int) o).ToString(CultureInfo.InvariantCulture);
            if (o is double)
                return ((double)o).ToString(CultureInfo.InvariantCulture);
            if (o is long)
                return ((long)o).ToString(CultureInfo.InvariantCulture);
            if (o is bool)
                return ((bool) o).ToString(CultureInfo.InvariantCulture);
            return o.ToString();
        }


        protected virtual void OnValueChangedEvent(ValueItem item)
        {
            ValueChangedEvent?.Invoke(this, item);
        }

        public void CopyFrom(ValueItemCollection values)
        {
            foreach (var item in values.Items)
            {
                this[item.Name] = item.Value;
            }
        }
    }
}
