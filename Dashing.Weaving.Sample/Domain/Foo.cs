﻿namespace Dashing.Weaving.Sample.Domain {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class Foo {
        public int FooId { get; set; }

        public string Name { get; set; }

        public bool IsBar { get; set; }

        public bool? IsRah { get; set; }

        public FooType Type { get; set; }

        public IEnumerable<Bar> Bars { get; set; }
    }

    public enum FooType {
        One,
        Two,
        Three
    }

    public class Bar {
        public Bar() {
            this.Ducks = new[] { new Duck { Id = 1 } };
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public Foo Foo { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? SomethingOrTother { get; set; }

        public IList<Duck> Ducks { get; set; }
    }

    public class Duck {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class Starship {
        private bool __foo;

        private bool __bar;

        public int Id { get; set; }

        //public bool Foo {
        //    get {
        //        return this.__foo;
        //    }

        //    set {
        //        this.__bar = true;
        //        this.__foo = value;
        //    }
        //}

        public bool Foo { get; set; }
        public bool GetBar() {
            return this.__bar;
        }
    }

    public class IveGotMethods {
        public int Id { get; set; }

        public string Name { get; set; }

        public override int GetHashCode() {
            return 42;
        }

        public override bool Equals(object obj) {
            return obj == null;
        }
    }
}