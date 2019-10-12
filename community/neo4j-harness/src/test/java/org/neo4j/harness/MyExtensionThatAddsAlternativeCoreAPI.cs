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
namespace Org.Neo4j.Harness
{
	using ExtensionType = Org.Neo4j.Kernel.extension.ExtensionType;
	using Org.Neo4j.Kernel.extension;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

	// Similar to the MyExtensionThatAddsInjectable, this demonstrates a
	// non-public mechanism for adding new context components, but in this
	// case the goal is to provide alternative Core API's and as such it wraps
	// the old Core API.
	public class MyExtensionThatAddsAlternativeCoreAPI : KernelExtensionFactory<MyExtensionThatAddsAlternativeCoreAPI.Dependencies>
	{
		 public MyExtensionThatAddsAlternativeCoreAPI() : base(ExtensionType.DATABASE, "my-ext")
		 {
		 }

		 public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
		 {
			  dependencies.Procedures().registerComponent(typeof(MyCoreAPI), ctx => new MyCoreAPI(dependencies.GraphDatabaseAPI, dependencies.TxBridge(), dependencies.LogService().getUserLog(typeof(MyCoreAPI))), true);
			  return new LifecycleAdapter();
		 }

		 public interface Dependencies
		 {
			  LogService LogService();

			  Procedures Procedures();

			  GraphDatabaseAPI GraphDatabaseAPI { get; }

			  ThreadToStatementContextBridge TxBridge();

		 }
	}

}