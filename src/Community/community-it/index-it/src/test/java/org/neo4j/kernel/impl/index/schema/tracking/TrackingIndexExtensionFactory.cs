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
namespace Neo4Net.Kernel.Impl.Index.Schema.tracking
{
	using NativeLuceneFusionIndexProviderFactory20 = Neo4Net.Kernel.Api.Impl.Schema.NativeLuceneFusionIndexProviderFactory20;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Neo4Net.Kernel.extension;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;

	public class TrackingIndexExtensionFactory : KernelExtensionFactory<TrackingIndexExtensionFactory.Dependencies>
	{
		 private TrackingReadersIndexProvider _indexProvider;

		 public TrackingIndexExtensionFactory() : base("trackingIndex")
		 {
		 }

		 public interface Dependencies : NativeLuceneFusionIndexProviderFactory20.Dependencies
		 {
		 }

		 public override IndexProvider NewInstance( KernelContext context, Dependencies dependencies )
		 {
			 lock ( this )
			 {
				  if ( _indexProvider == null )
				  {
						IndexProvider indexProvider = ( new NativeLuceneFusionIndexProviderFactory20() ).newInstance(context, dependencies);
						this._indexProvider = new TrackingReadersIndexProvider( indexProvider );
				  }
				  return _indexProvider;
			 }
		 }

		 public virtual TrackingReadersIndexProvider IndexProvider
		 {
			 get
			 {
				  return _indexProvider;
			 }
		 }
	}

}