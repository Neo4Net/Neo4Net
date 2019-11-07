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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using CapableIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.CapableIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.api.index.IndexPopulationFailure.failure;

	public class FailedPopulatingIndexProxyFactory : FailedIndexProxyFactory
	{
		 private readonly CapableIndexDescriptor _capableIndexDescriptor;
		 private readonly IndexPopulator _populator;
		 private readonly string _indexUserDescription;
		 private readonly IndexCountsRemover _indexCountsRemover;
		 private readonly LogProvider _logProvider;

		 internal FailedPopulatingIndexProxyFactory( CapableIndexDescriptor capableIndexDescriptor, IndexPopulator populator, string indexUserDescription, IndexCountsRemover indexCountsRemover, LogProvider logProvider )
		 {
			  this._capableIndexDescriptor = capableIndexDescriptor;
			  this._populator = populator;
			  this._indexUserDescription = indexUserDescription;
			  this._indexCountsRemover = indexCountsRemover;
			  this._logProvider = logProvider;
		 }

		 public override IndexProxy Create( Exception failure )
		 {
			  return new FailedIndexProxy( _capableIndexDescriptor, _indexUserDescription, _populator, failure( failure ), _indexCountsRemover, _logProvider );
		 }
	}

}