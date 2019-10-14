using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api
{

	using IOUtils = Neo4Net.Io.IOUtils;
	using ResourceManager = Neo4Net.Kernel.api.ResourceManager;
	using ResourceCloseFailureException = Neo4Net.Kernel.Api.Exceptions.ResourceCloseFailureException;

	public class CloseableResourceManager : ResourceManager
	{
		 private ICollection<AutoCloseable> _closeableResources;

		 // ResourceTracker

		 public override void RegisterCloseableResource( AutoCloseable closeable )
		 {
			  if ( _closeableResources == null )
			  {
					_closeableResources = new List<AutoCloseable>( 8 );
			  }
			  _closeableResources.Add( closeable );
		 }

		 public override void UnregisterCloseableResource( AutoCloseable closeable )
		 {
			  if ( _closeableResources != null )
			  {
					_closeableResources.remove( closeable );
			  }
		 }

		 // ResourceManager

		 public override void CloseAllCloseableResources()
		 {
			  if ( _closeableResources != null )
			  {
					// Make sure we reset closeableResource before doing anything which may throw an exception that
					// _may_ result in a recursive call to this close-method
					ICollection<AutoCloseable> resourcesToClose = _closeableResources;
					_closeableResources = null;

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					IOUtils.close( ResourceCloseFailureException::new, resourcesToClose.toArray( new AutoCloseable[0] ) );
			  }
		 }
	}

}