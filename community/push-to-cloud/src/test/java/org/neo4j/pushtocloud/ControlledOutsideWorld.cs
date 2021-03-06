﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Org.Neo4j.Pushtocloud
{

	using NullOutsideWorld = Org.Neo4j.Commandline.admin.NullOutsideWorld;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.NullOutputStream.NULL_OUTPUT_STREAM;

	internal class ControlledOutsideWorld : NullOutsideWorld
	{
		 private readonly IList<string> _promptResponses = new List<string>();
		 private readonly IList<char[]> _passwordResponses = new List<char[]>();
		 private readonly FileSystemAbstraction _fs;
		 private readonly PrintStream _nullOutputStream = new PrintStream( NULL_OUTPUT_STREAM );
		 private int _promptResponseCursor;
		 private int _passwordResponseCursor;

		 internal ControlledOutsideWorld( FileSystemAbstraction fs )
		 {
			  this._fs = fs;
		 }

		 internal virtual ControlledOutsideWorld WithPromptResponse( string line )
		 {
			  _promptResponses.Add( line );
			  return this;
		 }

		 internal virtual ControlledOutsideWorld WithPasswordResponse( char[] password )
		 {
			  _passwordResponses.Add( password );
			  return this;
		 }

		 public override string PromptLine( string fmt, params object[] args )
		 {
			  if ( _promptResponseCursor < _promptResponses.Count )
			  {
					return _promptResponses[_promptResponseCursor++];
			  }
			  return "";
		 }

		 public override char[] PromptPassword( string fmt, params object[] args )
		 {
			  if ( _passwordResponseCursor < _passwordResponses.Count )
			  {
					return _passwordResponses[_passwordResponseCursor++];
			  }
			  return new char[0];
		 }

		 public override FileSystemAbstraction FileSystem()
		 {
			  return _fs;
		 }

		 public override PrintStream OutStream()
		 {
			  return _nullOutputStream;
		 }
	}

}