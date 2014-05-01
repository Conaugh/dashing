﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Configuration
{
    public class Map<T> : IMap<T>
    {
        public Map()
        {
            this.Columns = new List<Column>();
            this.Indexes = new List<IList<string>>();
        }

        public Type Type { get; set; }

        public string Table { get; set; }

        public string Schema { get; set; }

        public string PrimaryKey { get; set; }

        public bool IsPrimaryKeyDatabaseGenerated { get; set; }

        public IList<Column> Columns { get; set; }

        public IList<IList<string>> Indexes { get; set; }
    }
}