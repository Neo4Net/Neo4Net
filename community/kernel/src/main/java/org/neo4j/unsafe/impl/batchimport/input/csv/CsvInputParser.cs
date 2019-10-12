using System;

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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.input.csv
{

	using CharSeeker = Org.Neo4j.Csv.Reader.CharSeeker;
	using Org.Neo4j.Csv.Reader;
	using Extractors = Org.Neo4j.Csv.Reader.Extractors;
	using Mark = Org.Neo4j.Csv.Reader.Mark;
	using LongExtractor = Org.Neo4j.Csv.Reader.Extractors.LongExtractor;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using Entry = Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.Header.Entry;

	/// <summary>
	/// CSV data to input entity parsing logic. Parsed CSV data is fed into <seealso cref="InputEntityVisitor"/>.
	/// </summary>
	public class CsvInputParser : System.IDisposable
	{
		 private readonly CharSeeker _seeker;
		 private readonly Mark _mark = new Mark();
		 private readonly IdType _idType;
		 private readonly Header _header;
		 private readonly int _delimiter;
		 private readonly Collector _badCollector;
		 private readonly Extractor<string> _stringExtractor;

		 private long _lineNumber;

		 public CsvInputParser( CharSeeker seeker, int delimiter, IdType idType, Header header, Collector badCollector, Extractors extractors )
		 {
			  this._seeker = seeker;
			  this._delimiter = delimiter;
			  this._idType = idType;
			  this._header = header;
			  this._badCollector = badCollector;
			  this._stringExtractor = extractors.String();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean next(org.neo4j.unsafe.impl.batchimport.input.InputEntityVisitor visitor) throws java.io.IOException
		 internal virtual bool Next( InputEntityVisitor visitor )
		 {
			  _lineNumber++;
			  int i = 0;
			  Entry entry = null;
			  Entry[] entries = _header.entries();
			  try
			  {
					bool doContinue = true;
					for ( i = 0; i < entries.Length && doContinue; i++ )
					{
						 entry = entries[i];
						 if ( !_seeker.seek( _mark, _delimiter ) )
						 {
							  if ( i > 0 )
							  {
									throw new UnexpectedEndOfInputException( "Near " + _mark );
							  }
							  // We're just at the end
							  return false;
						 }

						 switch ( entry.Type() )
						 {
						 case ID:
							  if ( _seeker.tryExtract( _mark, entry.Extractor() ) )
							  {
									switch ( _idType.innerEnumValue )
									{
									case Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.IdType.InnerEnum.STRING:
									case Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.IdType.InnerEnum.INTEGER:
										 object idValue = entry.Extractor().value();
										 doContinue = visitor.Id( idValue, entry.Group() );
										 if ( !string.ReferenceEquals( entry.Name(), null ) )
										 {
											  doContinue = visitor.Property( entry.Name(), idValue );
										 }
										 break;
									case Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.IdType.InnerEnum.ACTUAL:
										 doContinue = visitor.Id( ( ( Extractors.LongExtractor ) entry.Extractor() ).longValue() );
										 break;
									default:
										throw new System.ArgumentException( _idType.name() );
									}
							  }
							  break;
						 case START_ID:
							  if ( _seeker.tryExtract( _mark, entry.Extractor() ) )
							  {
									switch ( _idType.innerEnumValue )
									{
									case Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.IdType.InnerEnum.STRING:
										 doContinue = visitor.StartId( entry.Extractor().value(), entry.Group() );
										 break;
									case Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.IdType.InnerEnum.INTEGER:
										 doContinue = visitor.StartId( entry.Extractor().value(), entry.Group() );
										 break;
									case Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.IdType.InnerEnum.ACTUAL:
										 doContinue = visitor.StartId( ( ( Extractors.LongExtractor ) entry.Extractor() ).longValue() );
										 break;
									default:
										throw new System.ArgumentException( _idType.name() );
									}
							  }
							  break;
						 case END_ID:
							  if ( _seeker.tryExtract( _mark, entry.Extractor() ) )
							  {
									switch ( _idType.innerEnumValue )
									{
									case Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.IdType.InnerEnum.STRING:
										 doContinue = visitor.EndId( entry.Extractor().value(), entry.Group() );
										 break;
									case Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.IdType.InnerEnum.INTEGER:
										 doContinue = visitor.EndId( entry.Extractor().value(), entry.Group() );
										 break;
									case Org.Neo4j.@unsafe.Impl.Batchimport.input.csv.IdType.InnerEnum.ACTUAL:
										 doContinue = visitor.EndId( ( ( Extractors.LongExtractor ) entry.Extractor() ).longValue() );
										 break;
									default:
										throw new System.ArgumentException( _idType.name() );
									}
							  }
							  break;
						  case TYPE:
							  if ( _seeker.tryExtract( _mark, entry.Extractor() ) )
							  {
									doContinue = visitor.Type( ( string ) entry.Extractor().value() );
							  }
							  break;
						 case PROPERTY:
							  if ( _seeker.tryExtract( _mark, entry.Extractor(), entry.OptionalParameter() ) )
							  {
									// TODO since PropertyStore#encodeValue takes Object there's no point splitting up
									// into different primitive types
									object value = entry.Extractor().value();
									if ( !IsEmptyArray( value ) )
									{
										 doContinue = visitor.Property( entry.Name(), value );
									}
							  }
							  break;
						 case LABEL:
							  if ( _seeker.tryExtract( _mark, entry.Extractor() ) )
							  {
									object labelsValue = entry.Extractor().value();
									if ( labelsValue.GetType().IsArray )
									{
										 doContinue = visitor.Labels( ( string[] ) labelsValue );
									}
									else
									{
										 doContinue = visitor.Labels( new string[] { ( string ) labelsValue } );
									}
							  }
							  break;
						 case IGNORE:
							  break;
						 default:
							  throw new System.ArgumentException( entry.Type().ToString() );
						 }

						 if ( _mark.EndOfLine )
						 {
							  // We're at the end of the line, break and return an entity with what we have.
							  break;
						 }
					}

					while ( !_mark.EndOfLine )
					{
						 _seeker.seek( _mark, _delimiter );
						 if ( doContinue )
						 {
							  _seeker.tryExtract( _mark, _stringExtractor, entry.OptionalParameter() );
							  _badCollector.collectExtraColumns( _seeker.sourceDescription(), _lineNumber, _stringExtractor.value() );
						 }
					}
					visitor.EndOfEntity();
					return true;
			  }
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final RuntimeException e)
			  catch ( Exception e )
			  {
					string stringValue = null;
					try
					{
						 Extractors extractors = new Extractors( '?' );
						 if ( _seeker.tryExtract( _mark, extractors.String(), entry.OptionalParameter() ) )
						 {
							  stringValue = extractors.String().value();
						 }
					}
					catch ( Exception )
					{ // OK
					}

					string message = format( "ERROR in input" + "%n  data source: %s" + "%n  in field: %s" + "%n  for header: %s" + "%n  raw field value: %s" + "%n  original error: %s", _seeker, entry + ":" + ( i + 1 ), _header, !string.ReferenceEquals( stringValue, null ) ? stringValue : "??", e.Message );

					if ( e is InputException )
					{
						 throw Exceptions.withMessage( e, message );
					}
					throw new InputException( message, e );
			  }
		 }

		 private static bool IsEmptyArray( object value )
		 {
			  return value.GetType().IsArray && Array.getLength(value) == 0;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _seeker.Dispose();
		 }
	}

}