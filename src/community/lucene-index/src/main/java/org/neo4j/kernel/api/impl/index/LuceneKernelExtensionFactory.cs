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
namespace Neo4Net.Kernel.Api.Impl.Index
{
	using LuceneIndexImplementation = Neo4Net.Index.impl.lucene.@explicit.LuceneIndexImplementation;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using IndexProviders = Neo4Net.Kernel.spi.explicitindex.IndexProviders;

	public class LuceneKernelExtensionFactory : KernelExtensionFactory<LuceneKernelExtensionFactory.Dependencies>
	{
		 public interface Dependencies
		 {
			  Config Config { get; }

			  IndexProviders IndexProviders { get; }

			  IndexConfigStore IndexStore { get; }

			  FileSystemAbstraction FileSystem();
		 }

		 public LuceneKernelExtensionFactory() : base(ExtensionType.DATABASE, LuceneIndexImplementation.SERVICE_NAME)
		 {
		 }

		 public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
		 {
			  return new LuceneKernelExtension( context.Directory(), dependencies.Config, dependencies.getIndexStore, dependencies.FileSystem(), dependencies.IndexProviders, context.DatabaseInfo().OperationalMode );
		 }
	}

}