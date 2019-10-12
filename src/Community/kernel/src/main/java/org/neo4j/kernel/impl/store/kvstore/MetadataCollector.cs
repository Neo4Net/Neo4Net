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
namespace Neo4Net.Kernel.impl.store.kvstore
{


	internal abstract class MetadataCollector : Metadata, EntryVisitor<BigEndianByteArrayBuffer>
	{
		public abstract bool Visit( Buffer key, Buffer value );
		 private static readonly sbyte[] _noData = new sbyte[0];
		 private readonly int _entriesPerPage;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final HeaderField<?>[] headerFields;
		 private readonly HeaderField<object>[] _headerFields;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<HeaderField<?>, int> headerIndexes = new java.util.HashMap<>();
		 private readonly IDictionary<HeaderField<object>, int> _headerIndexes = new Dictionary<HeaderField<object>, int>();
		 private readonly object[] _headerValues;
		 private int _header;
		 private int _data;
		 private State _state = State.ExpectingFormatSpecifier;
		 private sbyte[] _catalogue = _noData;
		 private readonly ReadableBuffer _expectedFormat;

		 internal MetadataCollector<T1>( int entriesPerPage, HeaderField<T1>[] headerFields, ReadableBuffer expectedFormat )
		 {
			  this._entriesPerPage = entriesPerPage;
			  this._headerFields = headerFields = headerFields.Clone();
			  this._expectedFormat = expectedFormat;
			  this._headerValues = new object[headerFields.Length];
			  for ( int i = 0; i < headerFields.Length; i++ )
			  {
					_headerIndexes[requireNonNull( headerFields[i], "header field" )] = i;
			  }
		 }

		 public override string ToString()
		 {
			  return "MetadataCollector[" + _state + "]";
		 }

		 public override Headers Headers()
		 {
			  return Headers.IndexedHeaders( _headerIndexes, _headerValues.Clone() );
		 }

		 public override bool Visit( BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value )
		 {
			  return _state.visit( this, key, value );
		 }

		 private void ReadHeader( int offset, BigEndianByteArrayBuffer value )
		 {
			  _headerValues[offset] = _headerFields[offset].read( value );
		 }

		 private void ReadData( BigEndianByteArrayBuffer key )
		 {
			  if ( ( ( _header + _data ) % _entriesPerPage ) == 1 || _data == 1 )
			  { // first entry in (a new) page, extend the catalogue
					int oldLen = _catalogue.Length;
					_catalogue = Arrays.copyOf( _catalogue, oldLen + 2 * key.Size() );
					key.DataTo( _catalogue, oldLen ); // write the first key of the page into the catalogue
			  }
			  // always update the catalogue with the last entry (seen) for the page
			  key.DataTo( _catalogue, _catalogue.Length - key.Size() );
		 }

		 internal abstract bool VerifyFormatSpecifier( ReadableBuffer value );

		 internal virtual ReadableBuffer ExpectedFormat()
		 {
			  return _expectedFormat;
		 }

		 internal override sbyte[] PageCatalogue()
		 {
			  return _catalogue;
		 }

		 internal override int HeaderEntries()
		 {
			  return _header;
		 }

		 internal override int TotalEntries()
		 {
			  return _header + _data;
		 }

		 private abstract class State
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           expecting_format_specifier { boolean visit(MetadataCollector collector, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value) { return readFormatSpecifier(collector, key, value); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           expecting_header { boolean visit(MetadataCollector collector, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value) { if(!key.allZeroes()) { throw new IllegalStateException("Expecting at least one header after the format specifier."); } if(value.allZeroes()) { int header = ++collector.header; assert header == 2 : "End-of-header markers are always the second header after the format specifier."; if(collector.headerFields.length > 0) { throw new IllegalStateException("Expected " + collector.headerFields.length + " header fields, none seen."); } collector.state = reading_data; return true; } else { return(collector.state = reading_header).visit(collector, key, value); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           reading_header { boolean visit(MetadataCollector collector, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value) { if(key.allZeroes()) { if(value.minusOneAtTheEnd()) { collector.state = done; return false; } if(collector.header > collector.headerFields.length) { throw new IllegalStateException("Too many header fields, expected only " + collector.headerFields.length); } int header = collector.header - 1; collector.header++; collector.readHeader(header, value); return true; } else { if(collector.headerFields.length >= collector.header) { throw new IllegalStateException("Expected " + collector.headerFields.length + " header fields, only " + (collector.header - 1) + " seen."); } return(collector.state = reading_data).visit(collector, key, value); } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           reading_data { boolean visit(MetadataCollector collector, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value) { if(key.allZeroes()) { long encodedEntries = value.getIntegerFromEnd(); long entries = encodedEntries == -1 ? 0 : encodedEntries; if(entries != collector.data) { collector.state = in_error; throw new IllegalStateException("Number of data entries does not match. (counted=" + collector.data + ", trailer=" + entries + ")"); } collector.state = done; return false; } else { collector.data++; collector.readData(key); return true; } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           done { boolean visit(MetadataCollector collector, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value) { throw new IllegalStateException("Metadata collection has completed."); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           in_error { boolean visit(MetadataCollector collector, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value) { throw new IllegalStateException("Metadata collection has failed."); } };

			  private static readonly IList<State> valueList = new List<State>();

			  static State()
			  {
				  valueList.Add( expecting_format_specifier );
				  valueList.Add( expecting_header );
				  valueList.Add( reading_header );
				  valueList.Add( reading_data );
				  valueList.Add( done );
				  valueList.Add( in_error );
			  }

			  public enum InnerEnum
			  {
				  expecting_format_specifier,
				  expecting_header,
				  reading_header,
				  reading_data,
				  done,
				  in_error
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private State( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract bool visit( MetadataCollector collector, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value );

			  internal static bool ReadFormatSpecifier( MetadataCollector collector, BigEndianByteArrayBuffer key, BigEndianByteArrayBuffer value )
			  {
					if ( !key.AllZeroes() )
					{
						 throw new System.InvalidOperationException( "Expecting a valid format specifier." );
					}
					if ( !collector.VerifyFormatSpecifier( value ) )
					{
						 collector.state = in_error;
						 throw new System.InvalidOperationException( format( "Format header/trailer has changed. " + "Expected format:`%s`, actual:`%s`.", collector.ExpectedFormat(), value ) );
					}

					try
					{
						 collector.header = 1;
						 return true;
					}
					finally
					{
						 collector.state = expecting_header;
					}
			  }

			 public static IList<State> values()
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

			 public static State valueOf( string name )
			 {
				 foreach ( State enumInstance in State.valueList )
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

}