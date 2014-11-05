﻿namespace Dashing.Configuration {
    using System;
    using System.Data;
    using System.Linq;

    using Dashing.Extensions;

    /// <summary>
    ///     The column.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class Column<T> : IColumn {
        public Column() {
            this.Type = typeof(T);
        }

        /// <summary>
        ///     Gets the type.
        /// </summary>
        public virtual Type Type { get; private set; }

        public IMap Map { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        private DbType dbType;

        /// <summary>
        ///     Gets or sets the db type.
        /// </summary>
        public DbType DbType {
            get {
                return this.Relationship == RelationshipType.ManyToOne ? this.Map.Configuration.GetMap(this.Type).PrimaryKey.DbType : this.dbType;
            }

            set {
                this.dbType = value;
            }
        }

        /// <summary>
        ///     Gets or sets the db name.
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        ///     Gets or sets the precision.
        /// </summary>
        public byte Precision { get; set; }

        /// <summary>
        ///     Gets or sets the scale.
        /// </summary>
        public byte Scale { get; set; }

        public bool MaxLength { get; set; }

        /// <summary>
        ///     Gets or sets the length.
        /// </summary>
        public ushort Length { get; set; }

        private string defaultValue;

        /// <summary>
        ///     The default value for the db column
        /// </summary>
        public string Default {
            get {
                return this.defaultValue
                       ?? (this.IsPrimaryKey || this.Relationship != RelationshipType.None ? null : this.DbType.DefaultFor(this.IsNullable));
            }

            set {
                this.defaultValue = value;
            }
        }

        /// <summary>
        ///     Gets or sets whether the column is nullable
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        ///     Gets or sets the whether the column is the primary key
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        ///     Gets or sets whether the column is auto generated
        /// </summary>
        public bool IsAutoGenerated { get; set; }

        /// <summary>
        ///     Indicates whether the column will be ignored for all queries and schema generation
        /// </summary>
        public bool IsIgnored { get; set; }

        /// <summary>
        ///     Indicates whether the column will be excluded from select queries unless specifically requested
        /// </summary>
        public bool IsExcludedByDefault { get; set; }

        public int FetchId { get; set; }

        /// <summary>
        ///     Gets or sets the relationship.
        /// </summary>
        public RelationshipType Relationship { get; set; }

        private IColumn childColumn;

        private readonly object childColumnLock = new object();

        public IColumn ChildColumn {
            get {
                if (this.Relationship != RelationshipType.OneToMany) {
                    throw new InvalidOperationException("You should only access the ChildColumn on a OneToMany property");
                }

                if (this.childColumn == null) {
                    lock (this.childColumnLock) {
                        if (this.childColumn == null) {
                            // if we have a parent reference key use that
                            var childType = this.Type.GetGenericArguments()[0];
                            var parentType = this.Map.Type;
                            if (this.ChildColumnName != null) {
                                this.childColumn = this.Map.Configuration.GetMap(childType).Columns[this.ChildColumnName];
                                if (this.childColumn == null || this.childColumn.Type != parentType) {
                                    throw new InvalidOperationException("You must map collections to a property of the correct type");
                                }
                            }
                            else {
                                // map by convention
                                var possibleColumns = this.Map.Configuration.GetMap(childType).Columns.Where(c => c.Value.Type == parentType).ToList();
                                if (possibleColumns.Count == 1) {
                                    this.childColumn = possibleColumns.First().Value;
                                }
                                else {
                                    if (possibleColumns.Count == 0) {
                                        throw new InvalidOperationException(
                                            "All OneToMany collections must be directional. You should add a property of type " + parentType.Name
                                            + " to the child type " + childType.Name);
                                    }

                                    throw new InvalidOperationException(
                                        "The child type \"" + childType.Name + "\" has more than one property of type " + parentType.Name
                                        + ". Please specify which property this column maps to using IConfiguration.Setup().Property().MapsTo()");
                                }
                            }
                        }
                    }
                }

                return this.childColumn;
            }

            set {
                this.childColumn = value;
            }
        }

        public string ChildColumnName { get; set; }

        /// <summary>
        ///     The from.
        /// </summary>
        /// <param name="column">
        ///     The column.
        /// </param>
        /// <returns>
        ///     The <see cref="Column" />.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// </exception>
        public static Column<T> From(IColumn column) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            if (typeof(T) != column.Type) {
                throw new ArgumentException("The argument does not represent a column of the correct generic type");
            }

            return new Column<T> {
                                     Name = column.Name,
                                     Map = column.Map,
                                     DbType = column.DbType,
                                     DbName = column.DbName,
                                     Precision = column.Precision,
                                     Scale = column.Scale,
                                     Length = column.Length,
                                     IsNullable = column.IsNullable,
                                     IsPrimaryKey = column.IsPrimaryKey,
                                     IsAutoGenerated = column.IsAutoGenerated,
                                     IsIgnored = column.IsIgnored,
                                     IsExcludedByDefault = column.IsExcludedByDefault,
                                     FetchId = column.FetchId,
                                     Relationship = column.Relationship,
                                 };
        }

        public IMap ParentMap {
            get {
                if (this.Relationship != RelationshipType.ManyToOne) {
                    throw new InvalidOperationException("A ParentMap only exists on a ManyToOne relationship");
                }

                return this.Map.Configuration.GetMap(this.Type);
            }
        }
    }
}