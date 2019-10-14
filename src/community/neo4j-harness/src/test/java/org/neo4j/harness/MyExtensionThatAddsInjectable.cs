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
	using Neo4Net.Kernel.extension;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	// This ensures a non-public mechanism for adding new context components
	// to Procedures is testable via Harness. While this is not public API,
	// this is a vital mechanism to cover use cases Procedures need to cover,
	// and is in place as an approach that should either eventually be made
	// public, or the relevant use cases addressed in other ways.
	public class MyExtensionThatAddsInjectable : KernelExtensionFactory<MyExtensionThatAddsInjectable.Dependencies>
	{
		 public MyExtensionThatAddsInjectable() : base("my-ext")
		 {
		 }

		 public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
		 {
			  dependencies.Procedures().registerComponent(typeof(SomeService), ctx => new SomeService(), true);
			  return new LifecycleAdapter();
		 }

		 public interface Dependencies
		 {
			  Procedures Procedures();
		 }
	}

}