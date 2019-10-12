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
namespace Org.Neo4j.Kernel.Api.Impl.Index
{
	using LuceneIndexImplementation = Org.Neo4j.Index.impl.lucene.@explicit.LuceneIndexImplementation;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ExtensionType = Org.Neo4j.Kernel.extension.ExtensionType;
	using Org.Neo4j.Kernel.extension;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using IndexProviders = Org.Neo4j.Kernel.spi.explicitindex.IndexProviders;

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