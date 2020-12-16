using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Simplic.SignalR.Ado.Net.Client
{
    public class SignalRDbParameterCollection : DbParameterCollection
    {
        private List<object> parameters = new List<object>();

        public override int Count => parameters.Count;

        public override object SyncRoot => throw new NotImplementedException();

        public override int Add(object value)
        {
            parameters.Add(value);

            // TODO: Really? Or check whether really added
            return 1;
        }

        public override void AddRange(Array values)
        {
            foreach (var value in values)
                parameters.Add(value);
        }

        public override void Clear() => parameters.Clear();

        public override bool Contains(object value) => parameters.Contains(value);

        public override bool Contains(string value) => parameters.Contains(value);

        public override void CopyTo(Array array, int index)
        {
            int i = 0;
            foreach (var parameter in parameters)
            {
                array.SetValue(parameter, index + i);
                i++;
            }
        }

        public override IEnumerator GetEnumerator() => parameters.GetEnumerator();

        public override int IndexOf(object value) => parameters.IndexOf(value);

        public override int IndexOf(string parameterName)
        {
            var parameter = GetParameter(parameterName);
            if (parameter != null)
                return parameters.IndexOf(parameter);

            return -1;
        }

        public override void Insert(int index, object value) => parameters.Insert(index, value);

        public override void Remove(object value) => parameters.Remove(value);

        public override void RemoveAt(int index) => parameters.RemoveAt(index);

        public override void RemoveAt(string parameterName)
        {
            var parameter = GetParameter(parameterName);
            if (parameter != null)
                parameters.Remove(parameter);
        }

        protected override DbParameter GetParameter(int index) => parameters[index] as DbParameter;

        protected override DbParameter GetParameter(string parameterName)
        {
            return parameters.OfType<DbParameter>().FirstOrDefault(x => x.ParameterName == parameterName);
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            RemoveAt(index);
            Insert(index, value);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var index = IndexOf(parameterName);
            if (index != -1)
            {
                RemoveAt(parameterName);
                Insert(index, value);
            }
            else
            {
                Add(value);
            }
        }
    }
}
