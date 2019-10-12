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
namespace Neo4Net.Test.extension
{
	using AfterEachCallback = org.junit.jupiter.api.extension.AfterEachCallback;
	using ExtensionContext = org.junit.jupiter.api.extension.ExtensionContext;
	using Namespace = org.junit.jupiter.api.extension.ExtensionContext.Namespace;

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

	internal abstract class FileSystemExtension<T> : StatefullFieldExtension<T>, AfterEachCallback where T : Neo4Net.Io.fs.FileSystemAbstraction
	{
		 internal const string FILE_SYSTEM = "fileSystem";
		 internal static readonly ExtensionContext.Namespace FileSystemNamespace = ExtensionContext.Namespace.create( FILE_SYSTEM );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void afterEach(org.junit.jupiter.api.extension.ExtensionContext context) throws Exception
		 public override void AfterEach( ExtensionContext context )
		 {
			  getStoredValue( context ).close();
			  removeStoredValue( context );
		 }

		 protected internal override string FieldKey
		 {
			 get
			 {
				  return FILE_SYSTEM;
			 }
		 }

		 protected internal override ExtensionContext.Namespace NameSpace
		 {
			 get
			 {
				  return FileSystemNamespace;
			 }
		 }
	}

}