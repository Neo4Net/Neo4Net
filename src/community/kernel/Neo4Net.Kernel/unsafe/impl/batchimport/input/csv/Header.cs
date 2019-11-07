using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{

	using CharSeeker = Neo4Net.Csv.Reader.CharSeeker;
	using Neo4Net.Csv.Reader;
	using CSVHeaderInformation = Neo4Net.Values.Storable.CSVHeaderInformation;

	/// <summary>
	/// Header of tabular/csv data input, specifying meta data about values in each "column", for example
	/// semantically of which <seealso cref="Type"/> they are and which <seealso cref="Extractor type of value"/> they are.
	/// </summary>
	public class Header : ICloneable
	{
		 public interface Factory
		 {
			  /// <param name="dataSeeker"> <seealso cref="CharSeeker"/> containing the data. Usually there's a header for us
			  /// to read at the very top of it. </param>
			  /// <param name="configuration"> <seealso cref="Configuration"/> specific to the format of the data. </param>
			  /// <param name="idType"> type of values we expect the ids to be. </param>
			  /// <param name="groups"> <seealso cref="Groups"/> to register groups in. </param>
			  /// <returns> the created <seealso cref="Header"/>. </returns>
			  Header Create( CharSeeker dataSeeker, Configuration configuration, IdType idType, Groups groups );

			  /// <returns> whether or not this header is already defined. If this returns {@code false} then the header
			  /// will be read from the top of the data stream. </returns>
			  bool Defined { get; }
		 }

		 private readonly Entry[] _entries;

		 public Header( params Entry[] entries )
		 {
			  this._entries = entries;
		 }

		 public virtual Entry[] Entries()
		 {
			  return _entries;
		 }

		 public override string ToString()
		 {
			  return Arrays.ToString( _entries );
		 }

		 public override Header Clone()
		 {
			  Entry[] entries = new Entry[this._entries.Length];
			  for ( int i = 0; i < entries.Length; i++ )
			  {
					entries[i] = this._entries[i].clone();
			  }
			  return new Header( entries );
		 }

		 public class Entry : ICloneable
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string NameConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Type TypeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Group GroupConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Neo4Net.csv.reader.Extractor<?> extractor;
			  internal readonly Extractor<object> ExtractorConflict;
			  // This can be used to encapsulate the parameters set in the header for spatial and temporal columns
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly CSVHeaderInformation OptionalParameterConflict;

			  public Entry<T1>( string name, Type type, Group group, Extractor<T1> extractor )
			  {
					this.NameConflict = name;
					this.TypeConflict = type;
					this.GroupConflict = group;
					this.ExtractorConflict = extractor;
					this.OptionalParameterConflict = null;
			  }

			  public Entry<T1>( string name, Type type, Group group, Extractor<T1> extractor, CSVHeaderInformation optionalParameter )
			  {
					this.NameConflict = name;
					this.TypeConflict = type;
					this.GroupConflict = group;
					this.ExtractorConflict = extractor;
					this.OptionalParameterConflict = optionalParameter;
			  }

			  public override string ToString()
			  {
					if ( OptionalParameterConflict == null )
					{
						 return ( !string.ReferenceEquals( NameConflict, null ) ? NameConflict : "" ) + ":" + ( TypeConflict == Type.Property ? ExtractorConflict.name().ToLower() : TypeConflict.name() ) + (Group() != Neo4Net.@unsafe.Impl.Batchimport.input.Group_Fields.Global ? "(" + Group().name() + ")" : "");
					}
					else
					{
						 return ( !string.ReferenceEquals( NameConflict, null ) ? NameConflict : "" ) + ":" + ( TypeConflict == Type.Property ? ExtractorConflict.name().ToLower() + "[" + OptionalParameterConflict + "]" : TypeConflict.name() ) + (Group() != Neo4Net.@unsafe.Impl.Batchimport.input.Group_Fields.Global ? "(" + Group().name() + ")" : "");
					}
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Neo4Net.csv.reader.Extractor<?> extractor()
			  public virtual Extractor<object> Extractor()
			  {
					return ExtractorConflict;
			  }

			  public virtual Type Type()
			  {
					return TypeConflict;
			  }

			  public virtual Group Group()
			  {
					return GroupConflict != null ? GroupConflict : Neo4Net.@unsafe.Impl.Batchimport.input.Group_Fields.Global;
			  }

			  public virtual string Name()
			  {
					return NameConflict;
			  }

			  public virtual CSVHeaderInformation OptionalParameter()
			  {
					return OptionalParameterConflict;
			  }

			  public override int GetHashCode()
			  {
					const int prime = 31;
					int result = 1;
					if ( !string.ReferenceEquals( NameConflict, null ) )
					{
						 result = prime * result + NameConflict.GetHashCode();
					}
					result = prime * result + TypeConflict.GetHashCode();
					if ( GroupConflict != null )
					{
						 result = prime * result + GroupConflict.GetHashCode();
					}
					result = prime * result + ExtractorConflict.GetHashCode();
					return result;
			  }

			  public override bool Equals( object obj )
			  {
					if ( this == obj )
					{
						 return true;
					}
					if ( obj == null || this.GetType() != obj.GetType() )
					{
						 return false;
					}
					Entry other = ( Entry ) obj;
					return NullSafeEquals( NameConflict, other.NameConflict ) && TypeConflict == other.TypeConflict && NullSafeEquals( GroupConflict, other.GroupConflict ) && ExtractorEquals( ExtractorConflict, other.ExtractorConflict ) && NullSafeEquals( OptionalParameterConflict, other.OptionalParameterConflict );
			  }

			  public override Entry Clone()
			  {
					return new Entry( NameConflict, TypeConflict, GroupConflict, ExtractorConflict != null ? ExtractorConflict.clone() : null, OptionalParameterConflict );
			  }

			  internal virtual bool NullSafeEquals( object o1, object o2 )
			  {
					return o1 == null || o2 == null ? o1 == o2 : o1.Equals( o2 );
			  }

			  internal virtual bool ExtractorEquals<T1, T2>( Extractor<T1> first, Extractor<T2> other )
			  {
					if ( first == null || other == null )
					{
						 return first == other;
					}
					return first.GetType().Equals(other.GetType());
			  }
		 }
	}

}