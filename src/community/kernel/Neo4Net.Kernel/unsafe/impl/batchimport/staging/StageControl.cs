using System;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{

	/// <summary>
	/// Represents a means to control and coordinate lifecycle matters about a <seealso cref="Stage"/> and all its
	/// <seealso cref="Step steps"/>.
	/// </summary>
	public interface IStageControl
	{
		 void Panic( Exception cause );

		 void AssertHealthy();

		 void Recycle( object batch );

		 T reuse<T>( System.Func<T> fallback );
	}

}