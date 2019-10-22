using System.Text;

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

	using SourceTraceability = Neo4Net.Csv.Reader.SourceTraceability;
	using Entry = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header.Entry;

	/// <summary>
	/// <seealso cref="Deserialization"/> that writes the values to a <seealso cref="System.Text.StringBuilder"/>, suitable for piping the
	/// data straight down to a .csv file.
	/// </summary>
	public class StringDeserialization : Deserialization<string>
	{
		 private readonly StringBuilder _builder = new StringBuilder();
		 private readonly Configuration _config;
		 private int _field;

		 public StringDeserialization( Configuration config )
		 {
			  this._config = config;
		 }

		 public override void Handle( Entry entry, object value )
		 {
			  if ( _field > 0 )
			  {
					_builder.Append( _config.delimiter() );
			  }
			  if ( value != null )
			  {
					Stringify( value );
			  }
			  _field++;
		 }

		 private void Stringify( object value )
		 {
			  if ( value is string )
			  {
					string @string = ( string ) value;
					bool quote = @string.IndexOf( '.' ) != -1 || @string.IndexOf( _config.quotationCharacter() ) != -1;
					if ( quote )
					{
						 _builder.Append( _config.quotationCharacter() );
					}
					_builder.Append( @string );
					if ( quote )
					{
						 _builder.Append( _config.quotationCharacter() );
					}
			  }
			  else if ( value.GetType().IsArray )
			  {
					int length = Array.getLength( value );
					for ( int i = 0; i < length; i++ )
					{
						 object item = Array.get( value, i );
						 if ( i > 0 )
						 {
							  _builder.Append( _config.arrayDelimiter() );
						 }
						 Stringify( item );
					}
			  }
			  else if ( value is Number )
			  {
					Number number = ( Number ) value;
					if ( value is float? )
					{
						 _builder.Append( number.floatValue() );
					}
					else if ( value is double? )
					{
						 _builder.Append( number.doubleValue() );
					}
					else
					{
						 _builder.Append( number.longValue() );
					}
			  }
			  else if ( value is bool? )
			  {
					_builder.Append( ( ( bool? ) value ).Value );
			  }
			  else
			  {
					throw new System.ArgumentException( value.ToString() + " " + value.GetType().Name );
			  }
		 }

		 public override string Materialize()
		 {
			  return _builder.ToString();
		 }

		 public override void Clear()
		 {
			  _builder.Remove( 0, _builder.Length );
			  _field = 0;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static System.Func<org.Neo4Net.csv.reader.SourceTraceability,Deserialization<String>> factory(final Configuration config)
		 public static System.Func<SourceTraceability, Deserialization<string>> Factory( Configuration config )
		 {
			  return from => new StringDeserialization( config );
		 }
	}

}