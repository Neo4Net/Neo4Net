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
namespace Org.Neo4j.Kernel.impl.proc
{

	using DefaultParameterValue = Org.Neo4j.@internal.Kernel.Api.procs.DefaultParameterValue;
	using FieldSignature = Org.Neo4j.@internal.Kernel.Api.procs.FieldSignature;
	using DefaultValueMapper = Org.Neo4j.Kernel.impl.util.DefaultValueMapper;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using SequenceValue = Org.Neo4j.Values.SequenceValue;
	using ByteArray = Org.Neo4j.Values.Storable.ByteArray;
	using ByteValue = Org.Neo4j.Values.Storable.ByteValue;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.DefaultParameterValue.ntByteArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.proc.ParseUtil.parseList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.SequenceValue_IterationPreference.RANDOM_ACCESS;

	public class ByteArrayConverter : System.Func<string, DefaultParameterValue>, FieldSignature.InputMapper
	{

		 public override DefaultParameterValue Apply( string s )
		 {
			  string value = s.Trim();
			  if ( value.Equals( "null", StringComparison.OrdinalIgnoreCase ) )
			  {
					return ntByteArray( null );
			  }
			  else
			  {
					IList<long> values = parseList( value, typeof( Long ) );
					sbyte[] bytes = new sbyte[values.Count];
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 bytes[i] = values[i].byteValue();
					}
					return ntByteArray( bytes );
			  }
		 }

		 public override object Map( object input )
		 {
			  if ( input is sbyte[] )
			  {
					return input;
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (input instanceof java.util.List<?>)
			  if ( input is IList<object> )
			  {
					System.Collections.IList list = ( System.Collections.IList ) input;
					sbyte[] bytes = new sbyte[list.Count];
					for ( int a = 0; a < bytes.Length; a++ )
					{
						 object value = list[a];
						 if ( value is sbyte? )
						 {
							  bytes[a] = ( sbyte? ) value.Value;
						 }
						 else
						 {
							  throw new System.ArgumentException( "Cannot convert " + value + " to byte for input to procedure" );
						 }
					}
					return bytes;
			  }
			  else
			  {
					throw new System.ArgumentException( "Cannot convert " + input.GetType().Name + " to byte[] for input to procedure" );
			  }
		 }

		 public override AnyValue Map( AnyValue input )
		 {

			  if ( input is ByteArray )
			  {
					return input;
			  }
			  if ( input is SequenceValue )
			  {
					SequenceValue list = ( SequenceValue ) input;
					if ( list.IterationPreference() == RANDOM_ACCESS )
					{
						 sbyte[] bytes = new sbyte[list.Length()];
						 for ( int a = 0; a < bytes.Length; a++ )
						 {
							  bytes[a] = AsByte( list.Value( a ) );
						 }
						 return Values.byteArray( bytes );
					}
					else
					{
						 //this may have linear complexity, still worth doing it upfront
						 sbyte[] bytes = new sbyte[list.Length()];
						 int i = 0;
						 foreach ( AnyValue anyValue in list )
						 {
							  bytes[i++] = AsByte( anyValue );
						 }

						 return Values.byteArray( bytes );
					}
			  }
			  else
			  {
					throw new System.ArgumentException( "Cannot convert " + input.GetType().Name + " to byte[] for input to procedure" );
			  }
		 }

		 private sbyte AsByte( AnyValue value )
		 {
			  if ( value is ByteValue )
			  {
					return ( ( ByteValue ) value ).value();
			  }
			  else
			  {
					throw new System.ArgumentException( "Cannot convert " + value.Map( new DefaultValueMapper( null ) ) + " to byte for input to procedure" );
			  }
		 }
	}

}