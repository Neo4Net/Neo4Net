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
namespace Neo4Net.Kernel.Impl.Api.security
{
	using AuthorizationViolationException = Neo4Net.Graphdb.security.AuthorizationViolationException;
	using AccessMode = Neo4Net.@internal.Kernel.Api.security.AccessMode;

	/// <summary>
	/// Access mode that wraps an access mode with a wrapping access mode. The resulting access mode allows things based
	/// on both the original and the wrapping mode, while retaining the meta data of the original mode only.
	/// </summary>
	internal abstract class WrappedAccessMode : AccessMode
	{
		public abstract string Name();
		public abstract bool AllowsProcedureWith( string[] allowed );
		public abstract bool AllowsPropertyReads( int propertyKey );
		public abstract bool AllowsSchemaWrites();
		public abstract bool AllowsTokenCreates();
		public abstract bool AllowsWrites();
		public abstract bool AllowsReads();
		 protected internal readonly AccessMode Original;
		 protected internal readonly AccessMode Wrapping;

		 internal WrappedAccessMode( AccessMode original, AccessMode wrapping )
		 {
			  this.Original = original;
			  this.Wrapping = wrapping;
		 }

		 public override AuthorizationViolationException OnViolation( string msg )
		 {
			  return Wrapping.onViolation( msg );
		 }

		 public virtual bool Overridden
		 {
			 get
			 {
				  return true;
			 }
		 }
	}

}