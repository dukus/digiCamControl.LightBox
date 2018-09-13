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

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> at the specified variable.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="index">The variable name</param>
        /// <returns></returns>
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
            return Items.FirstOrDefault(item => item.Name == name);
        }


        /// <summary>
        /// Gets the variabe as string.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="defaultVal">The default value if the variable not found</param>
        /// <returns></returns>
        public string GetString(string name, string defaultVal = "")
        {
            foreach (var item in Items)
            {
                if (item.Name == name)
                    return item.Value;
            }
            return defaultVal;
        }

        /// <summary>
        /// Gets the variabe as int.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="defaultVal">The default value if the variable not found</param>
        /// <returns></returns>
        public int GetInt(string name, int defaultVal = 0)
        {
            var val = Get(name);
            if (val == null)
                return defaultVal;
            return Convert.ToInt32(val.Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the variabe as double.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="defaultVal">The default value if the variable not found</param>
        /// <returns></returns>

        public double GetDouble(string name, double defaultVal = 0)
        {
            var val = Get(name);
            if (val == null)
                return defaultVal;
            return Convert.ToDouble(val.Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets the variabe as bool.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <param name="defaultVal">The default value if the variable not found</param>
        /// <returns></returns>
        public bool GetBool(string name, bool defaultVal = false)
        {
            var val = Get(name);
            if (val == null)
                return defaultVal;
            return val.Value == "True";
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

        /// <summary>
        /// Copies all variables from a another variable connections
        /// </summary>
        /// <param name="values">The values.</param>
        public void CopyFrom(ValueItemCollection values)
        {
            foreach (var item in values.Items)
            {
                this[item.Name] = item.Value;
            }
        }
    }
}
