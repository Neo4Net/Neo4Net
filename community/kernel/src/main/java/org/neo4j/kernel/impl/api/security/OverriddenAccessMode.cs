﻿/*
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
namespace Org.Neo4j.Kernel.Impl.Api.security
{
	using AccessMode = Org.Neo4j.@internal.Kernel.Api.security.AccessMode;

	/// <summary>
	/// Access mode that overrides the original access mode with the overriding mode. Allows exactly what the overriding
	/// mode allows, while retaining the meta data of the original mode only.
	/// </summary>
	public class OverriddenAccessMode : WrappedAccessMode
	{
		 public OverriddenAccessMode( AccessMode original, AccessMode overriding ) : base( original, overriding )
		 {
		 }

		 public override bool AllowsReads()
		 {
			  return Wrapping.allowsReads();
		 }

		 public override bool AllowsWrites()
		 {
			  return Wrapping.allowsWrites();
		 }

		 public override bool AllowsTokenCreates()
		 {
			  return Wrapping.allowsTokenCreates();
		 }

		 public override bool AllowsSchemaWrites()
		 {
			  return Wrapping.allowsSchemaWrites();
		 }

		 public override bool AllowsPropertyReads( int propertyKey )
		 {
			  return Wrapping.allowsPropertyReads( propertyKey );
		 }

		 public override bool AllowsProcedureWith( string[] allowed )
		 {
			  return false;
		 }

		 public override string Name()
		 {
			  return Original.name() + " overridden by " + Wrapping.name();
		 }
	}

}