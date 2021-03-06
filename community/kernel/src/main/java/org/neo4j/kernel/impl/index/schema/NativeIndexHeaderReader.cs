﻿/*
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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using ExceptionUtils = org.apache.commons.lang3.exception.ExceptionUtils;


	using Header = Org.Neo4j.Index.@internal.gbptree.Header;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexPopulator.BYTE_FAILED;

	internal class NativeIndexHeaderReader : Header.Reader
	{
		 private readonly Header.Reader _additionalReader;
		 internal sbyte State;
		 internal string FailureMessage;

		 internal NativeIndexHeaderReader( Header.Reader additionalReader )
		 {
			  this._additionalReader = additionalReader;
		 }

		 public override void Read( ByteBuffer headerData )
		 {
			  try
			  {
					State = headerData.get();
					if ( State == BYTE_FAILED )
					{
						 FailureMessage = ReadFailureMessage( headerData );
					}
					else
					{
						 _additionalReader.read( headerData );
					}
			  }
			  catch ( BufferUnderflowException e )
			  {
					State = BYTE_FAILED;
					FailureMessage = format( "Could not read header, most likely caused by index not being fully constructed. Index needs to be recreated. Stacktrace:%n%s", ExceptionUtils.getStackTrace( e ) );
			  }
		 }

		 /// <summary>
		 /// Alternative header readers should react to FAILED indexes by using this, because their specific headers will have been
		 /// overwritten by the FailedHeaderWriter.
		 /// </summary>
		 internal static string ReadFailureMessage( ByteBuffer headerData )
		 {
			  short messageLength = headerData.Short;
			  sbyte[] failureMessageBytes = new sbyte[messageLength];
			  headerData.get( failureMessageBytes );
			  return StringHelper.NewString( failureMessageBytes, StandardCharsets.UTF_8 );
		 }
	}

}