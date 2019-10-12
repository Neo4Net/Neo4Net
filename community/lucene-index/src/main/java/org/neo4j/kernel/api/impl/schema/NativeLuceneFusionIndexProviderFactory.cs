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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{

	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using Org.Neo4j.Kernel.Impl.Index.Schema;
	using FusionIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.fusion.FusionIndexProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesBySubProvider;

	public abstract class NativeLuceneFusionIndexProviderFactory<DEPENDENCIES> : AbstractIndexProviderFactory<DEPENDENCIES> where DEPENDENCIES : Org.Neo4j.Kernel.Impl.Index.Schema.AbstractIndexProviderFactory.Dependencies
	{
		 internal NativeLuceneFusionIndexProviderFactory( string key ) : base( key )
		 {
		 }

		 protected internal override Type LoggingClass()
		 {
			  return typeof( FusionIndexProvider );
		 }

		 public static IndexDirectoryStructure.Factory SubProviderDirectoryStructure( File databaseDirectory, IndexProviderDescriptor descriptor )
		 {
			  IndexDirectoryStructure parentDirectoryStructure = directoriesByProvider( databaseDirectory ).forProvider( descriptor );
			  return directoriesBySubProvider( parentDirectoryStructure );
		 }
	}

}