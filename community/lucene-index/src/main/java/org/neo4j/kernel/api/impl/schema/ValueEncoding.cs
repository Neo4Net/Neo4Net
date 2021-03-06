﻿using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{
	using DoubleField = org.apache.lucene.document.DoubleField;
	using Field = org.apache.lucene.document.Field;
	using StringField = org.apache.lucene.document.StringField;
	using Term = Org.Apache.Lucene.Index.Term;
	using ConstantScoreQuery = org.apache.lucene.search.ConstantScoreQuery;
	using NumericRangeQuery = org.apache.lucene.search.NumericRangeQuery;
	using Query = org.apache.lucene.search.Query;
	using TermQuery = org.apache.lucene.search.TermQuery;

	using ArrayEncoder = Org.Neo4j.Kernel.Api.Index.ArrayEncoder;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.lucene.document.Field.Store.NO;

	/// <summary>
	/// Enumeration representing all possible property types with corresponding encodings and query structures for Lucene
	/// schema indexes.
	/// </summary>
	public abstract class ValueEncoding
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       Number { public String key() { return "number"; } boolean canEncode(org.neo4j.values.storable.Value value) { return org.neo4j.values.storable.Values.isNumberValue(value); } Field encodeField(String name, org.neo4j.values.storable.Value value) { return new org.apache.lucene.document.DoubleField(name, org.neo4j.values.storable.Values.coerceToDouble(value), NO); } void setFieldValue(org.neo4j.values.storable.Value value, org.apache.lucene.document.Field field) { field.setDoubleValue(org.neo4j.values.storable.Values.coerceToDouble(value)); } Query encodeQuery(org.neo4j.values.storable.Value value, int propertyNumber) { System.Nullable<double> doubleValue = org.neo4j.values.storable.Values.coerceToDouble(value); return new org.apache.lucene.search.ConstantScoreQuery(org.apache.lucene.search.NumericRangeQuery.newDoubleRange(key(propertyNumber), doubleValue, doubleValue, true, true)); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       Array { public String key() { return "array"; } boolean canEncode(org.neo4j.values.storable.Value value) { return org.neo4j.values.storable.Values.isArrayValue(value); } Field encodeField(String name, org.neo4j.values.storable.Value value) { return stringField(name, org.neo4j.kernel.api.index.ArrayEncoder.encode(value)); } void setFieldValue(org.neo4j.values.storable.Value value, org.apache.lucene.document.Field field) { field.setStringValue(org.neo4j.kernel.api.index.ArrayEncoder.encode(value)); } Query encodeQuery(org.neo4j.values.storable.Value value, int propertyNumber) { return new org.apache.lucene.search.ConstantScoreQuery(new org.apache.lucene.search.TermQuery(new org.apache.lucene.index.Term(key(propertyNumber), org.neo4j.kernel.api.index.ArrayEncoder.encode(value)))); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       Bool { public String key() { return "bool"; } boolean canEncode(org.neo4j.values.storable.Value value) { return org.neo4j.values.storable.Values.isBooleanValue(value); } Field encodeField(String name, org.neo4j.values.storable.Value value) { return stringField(name, value.prettyPrint()); } void setFieldValue(org.neo4j.values.storable.Value value, org.apache.lucene.document.Field field) { field.setStringValue(value.prettyPrint()); } Query encodeQuery(org.neo4j.values.storable.Value value, int propertyNumber) { return new org.apache.lucene.search.ConstantScoreQuery(new org.apache.lucene.search.TermQuery(new org.apache.lucene.index.Term(key(propertyNumber), value.prettyPrint()))); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       Spatial { public String key() { return "spatial"; } boolean canEncode(org.neo4j.values.storable.Value value) { return org.neo4j.values.storable.Values.isGeometryValue(value); } Field encodeField(String name, org.neo4j.values.storable.Value value) { org.neo4j.values.storable.PointValue pointVal = (org.neo4j.values.storable.PointValue) value; return stringField(name, pointVal.toIndexableString()); } void setFieldValue(org.neo4j.values.storable.Value value, org.apache.lucene.document.Field field) { org.neo4j.values.storable.PointValue pointVal = (org.neo4j.values.storable.PointValue) value; field.setStringValue(pointVal.toIndexableString()); } Query encodeQuery(org.neo4j.values.storable.Value value, int propertyNumber) { org.neo4j.values.storable.PointValue pointVal = (org.neo4j.values.storable.PointValue) value; return new org.apache.lucene.search.ConstantScoreQuery(new org.apache.lucene.search.TermQuery(new org.apache.lucene.index.Term(key(propertyNumber), pointVal.toIndexableString()))); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       Temporal { public String key() { return "temporal"; } boolean canEncode(org.neo4j.values.storable.Value value) { return org.neo4j.values.storable.Values.isTemporalValue(value); } Field encodeField(String name, org.neo4j.values.storable.Value value) { return stringField(name, value.prettyPrint()); } void setFieldValue(org.neo4j.values.storable.Value value, org.apache.lucene.document.Field field) { field.setStringValue(value.prettyPrint()); } Query encodeQuery(org.neo4j.values.storable.Value value, int propertyNumber) { return new org.apache.lucene.search.ConstantScoreQuery(new org.apache.lucene.search.TermQuery(new org.apache.lucene.index.Term(key(propertyNumber), value.prettyPrint()))); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       String { public String key() { return "string"; } boolean canEncode(org.neo4j.values.storable.Value value) { return true; } Field encodeField(String name, org.neo4j.values.storable.Value value) { return stringField(name, value.asObject().toString()); } void setFieldValue(org.neo4j.values.storable.Value value, org.apache.lucene.document.Field field) { field.setStringValue(value.asObject().toString()); } Query encodeQuery(org.neo4j.values.storable.Value value, int propertyNumber) { return new org.apache.lucene.search.ConstantScoreQuery(new org.apache.lucene.search.TermQuery(new org.apache.lucene.index.Term(key(propertyNumber), value.asObject().toString()))); } };

		 private static readonly IList<ValueEncoding> valueList = new List<ValueEncoding>();

		 static ValueEncoding()
		 {
			 valueList.Add( Number );
			 valueList.Add( Array );
			 valueList.Add( Bool );
			 valueList.Add( Spatial );
			 valueList.Add( Temporal );
			 valueList.Add( String );
		 }

		 public enum InnerEnum
		 {
			 Number,
			 Array,
			 Bool,
			 Spatial,
			 Temporal,
			 String
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private ValueEncoding( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 private static readonly ValueEncoding[] AllEncodings = values();

		 public abstract string key();

		 internal string key( int propertyNumber ) { if ( propertyNumber == 0 ) { return key(); } return propertyNumber + key(); } static int fieldPropertyNumber(string fieldName) { int index = 0; for (int i = 0; i < fieldName.length() && Character.isDigit(fieldName.charAt(i)); i++) { index++; } return index == 0 ? 0 : int.Parse(fieldName.substring(0, index)); } abstract bool canEncode(Org.Neo4j.Values.Storable.Value value);

		 internal abstract org.apache.lucene.document.Field encodeField( string name, Org.Neo4j.Values.Storable.Value value );

		 internal abstract void setFieldValue( Org.Neo4j.Values.Storable.Value value, org.apache.lucene.document.Field field );

		 internal abstract org.apache.lucene.search.Query encodeQuery( Org.Neo4j.Values.Storable.Value value, int propertyNumber );

		 public static ValueEncoding ForKey( string key )
		 {
			  foreach ( ValueEncoding encoding in _allEncodings )
			  {
					if ( key.EndsWith( encoding.Key(), StringComparison.Ordinal ) )
					{
						 return encoding;
					}
			  }
			  throw new System.ArgumentException( "Unknown key: " + key );
		 }

		 public static ValueEncoding ForValue( Org.Neo4j.Values.Storable.Value value )
		 {
			  foreach ( ValueEncoding encoding in _allEncodings )
			  {
					if ( encoding.canEncode( value ) )
					{
						 return encoding;
					}
			  }
			  throw new System.InvalidOperationException( "Unable to encode the value " + value );
		 }

		 private static org.apache.lucene.document.Field StringField( string identifier, string value )
		 {
			  return new StringField( identifier, value, NO );
		 }

		public static IList<ValueEncoding> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static ValueEncoding valueOf( string name )
		{
			foreach ( ValueEncoding enumInstance in ValueEncoding.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}