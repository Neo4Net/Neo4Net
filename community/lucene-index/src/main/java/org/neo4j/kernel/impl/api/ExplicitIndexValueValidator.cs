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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using ExplicitIndex = Org.Neo4j.Kernel.api.ExplicitIndex;
	using Org.Neo4j.Kernel.impl.util;

	/// <summary>
	/// Validates values added to an <seealso cref="ExplicitIndex"/> before commit.
	/// </summary>
	public class ExplicitIndexValueValidator : Validator<object>
	{
		 public static readonly ExplicitIndexValueValidator Instance = new ExplicitIndexValueValidator();

		 private ExplicitIndexValueValidator()
		 {
		 }

		 public override void Validate( object value )
		 {
			  if ( value == null )
			  {
					throw new System.ArgumentException( "Null value" );
			  }
			  if ( value is string )
			  {
					LuceneIndexValueValidator.Instance.validate( ( ( string )value ).GetBytes() );
			  }
			  if ( !( value is Number ) && string.ReferenceEquals( value.ToString(), null ) )
			  {
					throw new System.ArgumentException( "Value of type " + value.GetType() + " has null toString" );
			  }
		 }
	}

}