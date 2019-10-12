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
namespace Neo4Net.Kernel.impl.index
{
	using ExtensionType = Neo4Net.Kernel.extension.ExtensionType;
	using Neo4Net.Kernel.extension;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using IndexProviders = Neo4Net.Kernel.spi.explicitindex.IndexProviders;

	public class DummyIndexExtensionFactory : KernelExtensionFactory<DummyIndexExtensionFactory.Dependencies>
	{
		 public const string IDENTIFIER = "test-dummy-neo-index";
		 internal const string KEY_FAIL_ON_MUTATE = "fail_on_mutate";

		 public DummyIndexExtensionFactory() : base(ExtensionType.DATABASE, IDENTIFIER)
		 {
		 }

		 public interface Dependencies
		 {
			  IndexProviders IndexProviders { get; }
		 }

		 public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
		 {
			  IndexProviders indexProviders = dependencies.IndexProviders;
			  return new Extension( indexProviders );
		 }

		 private class Extension : LifecycleAdapter
		 {
			  internal readonly IndexProviders IndexProviders;

			  internal Extension( IndexProviders indexProviders )
			  {
					this.IndexProviders = indexProviders;
			  }

			  public override void Init()
			  {
					IndexProviders.registerIndexProvider( IDENTIFIER, new DummyIndexImplementation() );
			  }

			  public override void Shutdown()
			  {
					IndexProviders.unregisterIndexProvider( IDENTIFIER );
			  }
		 }
	}

}