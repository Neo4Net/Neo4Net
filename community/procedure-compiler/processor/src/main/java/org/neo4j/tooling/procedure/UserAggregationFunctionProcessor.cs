using System;

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
namespace Org.Neo4j.Tooling.procedure
{
	using AutoService = com.google.auto.service.AutoService;


	using UserAggregationFunction = Org.Neo4j.Procedure.UserAggregationFunction;
	using CustomNameExtractor = Org.Neo4j.Tooling.procedure.compilerutils.CustomNameExtractor;
	using TypeMirrorUtils = Org.Neo4j.Tooling.procedure.compilerutils.TypeMirrorUtils;
	using Org.Neo4j.Tooling.procedure.visitors;
	using UserAggregationFunctionVisitor = Org.Neo4j.Tooling.procedure.visitors.UserAggregationFunctionVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.tooling.procedure.CompilerOptions.IGNORE_CONTEXT_WARNINGS_OPTION;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AutoService(Processor.class) public class UserAggregationFunctionProcessor extends DuplicationAwareBaseProcessor<org.neo4j.procedure.UserAggregationFunction>
	public class UserAggregationFunctionProcessor : DuplicationAwareBaseProcessor<UserAggregationFunction>
	{

		 public static readonly Type<UserAggregationFunction> SupportedAnnotationType = typeof( UserAggregationFunction );

		 public UserAggregationFunctionProcessor() : base(SupportedAnnotationType, CustomNameExtractor(), processingEnvironment ->
		 {
		  {
				Types typeUtils = processingEnvironment.TypeUtils;
				Elements elementUtils = processingEnvironment.ElementUtils;
				TypeMirrorUtils typeMirrorUtils = new TypeMirrorUtils( typeUtils, elementUtils );

				return new UserAggregationFunctionVisitor( new FunctionVisitor<Org.Neo4j.Procedure.UserAggregationFunction>( SupportedAnnotationType, typeUtils, elementUtils, typeMirrorUtils, CustomNameExtractor(), processingEnvironment.Options.containsKey(IGNORE_CONTEXT_WARNINGS_OPTION) ), typeUtils );
		  });
		 }

		 private static System.Func<UserAggregationFunction, Optional<string>> CustomNameExtractor()
		 {
			  return function => CustomNameExtractor.getName( function.name, function.value );
		 }
	}

}