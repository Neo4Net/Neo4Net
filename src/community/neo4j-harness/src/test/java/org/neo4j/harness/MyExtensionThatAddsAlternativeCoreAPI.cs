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
namespace Neo4Net.Harness
{
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using LogService = Neo4Net.Logging.Internal.LogService;

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