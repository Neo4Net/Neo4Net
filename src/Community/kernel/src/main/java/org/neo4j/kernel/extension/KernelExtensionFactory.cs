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
namespace Neo4Net.Kernel.extension
{
	using Service = Neo4Net.Helpers.Service;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;

	public abstract class KernelExtensionFactory<DEPENDENCIES> : Service
	{
		 private readonly ExtensionType _extensionType;

		 protected internal KernelExtensionFactory( string key ) : this( ExtensionType.Global, key )
		 {
		 }

		 protected internal KernelExtensionFactory( ExtensionType extensionType, string key ) : base( key )
		 {
			  this._extensionType = extensionType;
		 }

		 /// <summary>
		 /// Create a new instance of this kernel extension.
		 /// </summary>
		 /// <param name="context"> the context the extension should be created for </param>
		 /// <param name="dependencies"> deprecated </param>
		 /// <returns> the <seealso cref="Lifecycle"/> for the extension </returns>
		 public abstract Lifecycle NewInstance( KernelContext context, DEPENDENCIES dependencies );

		 public override string ToString()
		 {
			  return "KernelExtension:" + this.GetType().Name + Keys;
		 }

		 internal virtual ExtensionType ExtensionType
		 {
			 get
			 {
				  return _extensionType;
			 }
		 }
	}

}