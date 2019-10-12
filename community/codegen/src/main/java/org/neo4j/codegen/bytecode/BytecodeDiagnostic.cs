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
namespace Org.Neo4j.Codegen.bytecode
{

	internal class BytecodeDiagnostic : Diagnostic<Void>
	{
		 private readonly string _message;

		 internal BytecodeDiagnostic( string message )
		 {
			  this._message = message;
		 }

		 public override string GetMessage( Locale locale )
		 {
			  return _message;
		 }

		 public override Kind Kind
		 {
			 get
			 {
				  return Kind.ERROR;
			 }
		 }

		 public override Void Source
		 {
			 get
			 {
				  return null;
			 }
		 }

		 public override long Position
		 {
			 get
			 {
				  return NOPOS;
			 }
		 }

		 public override long StartPosition
		 {
			 get
			 {
				  return NOPOS;
			 }
		 }

		 public override long EndPosition
		 {
			 get
			 {
				  return NOPOS;
			 }
		 }

		 public override long LineNumber
		 {
			 get
			 {
				  return NOPOS;
			 }
		 }

		 public override long ColumnNumber
		 {
			 get
			 {
				  return NOPOS;
			 }
		 }

		 public override string Code
		 {
			 get
			 {
				  return null;
			 }
		 }
	}

}