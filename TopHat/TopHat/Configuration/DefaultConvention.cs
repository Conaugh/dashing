﻿using System;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;

namespace TopHat.Configuration {
	// Dear ReSharper, these things were done on purpose so that people can extend off this
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	// ReSharper disable once MemberCanBePrivate.Global
	public class DefaultConvention : IConvention {
		private readonly ushort _stringLength;
		private readonly byte _decimalPrecision;
		private readonly byte _decimalScale;
		protected readonly PluralizationService Pluralizer;

		public DefaultConvention(ushort stringLength = 255, byte decimalPrecision = 18, byte decimalScale = 10) {
			_stringLength = stringLength;
			_decimalPrecision = decimalPrecision;
			_decimalScale = decimalScale;
			Pluralizer = PluralizationService.CreateService(new CultureInfo("en-GB")); // <-- Americans, back in your box.
		}

		public virtual string TableFor(Type entity) {
			return Pluralizer.Pluralize(entity.Name);
		}

		public virtual String SchemaFor(Type entity) {
			return null;
		}

		public virtual String PrimaryKeyOf(Type entity) {
			return entity.Name + "Id";
		}

		public virtual bool IsPrimaryKeyAutoGenerated(Type entity) {
			return true;
		}

		public ushort StringLengthFor(Type entity, string propertyName) {
			return _stringLength;
		}

		public byte DecimalPrecisionFor(Type entity, string propertyName) {
			return _decimalPrecision;
		}

		public byte DecimalScaleFor(Type entity, string propertyName) {
			return _decimalScale;
		}
	}
}