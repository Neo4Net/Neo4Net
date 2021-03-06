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
namespace Org.Neo4j.Values.Storable
{

	using Org.Neo4j.Values;

	public sealed class InputMappingStructureBuilder<Input, Internal, Result> : StructureBuilder<Input, Result>
	{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <R> org.neo4j.values.StructureBuilder<Object,R> fromValues(org.neo4j.values.StructureBuilder<? super Value,R> builder)
		 public static StructureBuilder<object, R> FromValues<R, T1>( StructureBuilder<T1> builder )
		 {
			  return Mapping( Values.of, builder );
		 }

		 public static StructureBuilder<I, O> Mapping<I, N, O>( System.Func<I, N> mapping, StructureBuilder<N, O> builder )
		 {
			  return new InputMappingStructureBuilder<I, O>( mapping, builder );
		 }

		 private readonly System.Func<Input, Internal> _mapping;
		 private readonly StructureBuilder<Internal, Result> _builder;

		 private InputMappingStructureBuilder( System.Func<Input, Internal> mapping, StructureBuilder<Internal, Result> builder )
		 {
			  this._mapping = mapping;
			  this._builder = builder;
		 }

		 public override StructureBuilder<Input, Result> Add( string field, Input value )
		 {
			  _builder.add( field, _mapping.apply( value ) );
			  return this;
		 }

		 public override Result Build()
		 {
			  return _builder.build();
		 }
	}

}